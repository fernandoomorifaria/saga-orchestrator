#!/bin/bash

sleep 10

count=$(/opt/kafka/bin/kafka-topics.sh --list --bootstrap-server localhost:9092 2>/dev/null | wc -l)

if [ "$count" -le 1 ]; then
  topics=(
    "orders"
    "order-replies"
    "orchestrator"
    "inventory"
    "inventory-replies"
    "payments"
    "payments-replies"
  )

  for topic in "${topics[@]}"; do
    /opt/kafka/bin/kafka-topics.sh --create --topic "$topic" --partitions 1 --replication-factor 1 --bootstrap-server localhost:9092 --if-not-exists 2>/dev/null
  done
fi

exit 0
