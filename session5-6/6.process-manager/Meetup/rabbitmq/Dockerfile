FROM curlimages/curl AS plugin
RUN curl -L https://github.com/rabbitmq/rabbitmq-delayed-message-exchange/releases/download/3.8.9/rabbitmq_delayed_message_exchange-3.8.9-0199d11c.ez > /home/curl_user/rabbitmq_delayed_message_exchange-3.8.9.ez 

FROM rabbitmq:3.8.9-management
COPY --from=plugin /home/curl_user/rabbitmq_delayed_message_exchange-3.8.9.ez $RABBITMQ_HOME/plugins/rabbitmq_delayed_message_exchange-3.8.9.ez
RUN rabbitmq-plugins enable --offline rabbitmq_delayed_message_exchange rabbitmq_consistent_hash_exchange rabbitmq_prometheus rabbitmq_shovel rabbitmq_shovel_management
