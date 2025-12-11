import 'dotenv/config';
import { drizzle } from 'drizzle-orm/node-postgres';
import { productsTable } from './db/schema.ts';
import { KafkaJS } from '@confluentinc/kafka-javascript';
import { eq, or, sql } from 'drizzle-orm';

const db = drizzle(process.env.DATABASE_URL);

const producer = new KafkaJS.Kafka().producer({
  'bootstrap.servers': process.env.KAFKA_SERVER
});

const consumer = new KafkaJS.Kafka().consumer({
  'bootstrap.servers': process.env.KAFKA_SERVER,
  'group.id': 'inventory-consumer'
});

const topic = process.env.KAFKA_TOPIC;

await producer.connect();
await consumer.connect();

await consumer.subscribe({ topics: [topic] });

interface ReserveInventoryCommand {
  sagaId: string;
  orderId: string
  type: string
  productId: number
}

interface Compensate {

}

interface Event {
  sagaId: string;
  orderId: string;
  type: string;
  success: boolean;
}

await consumer.run({
  eachMessage: async ({ topic, partition, message }) => {
    if (topic === 'inventory') {
      const command: ReserveInventoryCommand = JSON.parse(message.value.toString());
      const orderId = command.orderId;
      const productId = command.productId;

      if (command.type === 'ReserveInventory') {
        const [result] = await db
          .select({ quantity: productsTable.quantity })
          .from(productsTable)
          .where(eq(productsTable.id, productId))
          .limit(1);

        if (result.quantity > 0) {
          await db.update(productsTable)
            .set({ quantity: sql`${productsTable.quantity} - 1` })
            .where(eq(productsTable.id, productId));

          let event: Event = {
            sagaId: command.sagaId,
            orderId: orderId,
            type: 'InventoryReserved',
            success: true
          }

          await producer.send({
            topic: process.env.KAFKA_REPLY_TOPIC,
            messages: [
              {
                key: orderId,
                value: JSON.stringify(event)
              }
            ]
          })
        } else {
          let event: Event = {
            sagaId: command.sagaId,
            orderId: orderId,
            type: 'OutOfStock',
            success: false
          }

          await producer.send({
            topic: process.env.KAFKA_REPLY_TOPIC,
            messages: [
              {
                key: orderId,
                value: JSON.stringify(event)
              }
            ]
          });
        }
      }
    }
    else if (topic === 'compensate') {

    }
  }
});
