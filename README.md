# h2h-eng-gp-processor

The germ plasm processor will load and filter entry means for one germ plasm, applying some filters during the API call and some afterwards on the returned values.

Running within Lambda, it will create cache entries, send SQS messages to the engine responsible for generatign the head to head comparisons and also SNS messages to initiate N lambda instances of the head to head comparison generator engine.
