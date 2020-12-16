# Scavenger Hunt Backend

The Backend consists of the following components:

* RESTful API, running a python flask application on AWS LAMBDA and database on AWS DynamoDB, for the core game related tasks. [AWS LAMBDA](aws_lambda/README.md)

* Fabric RESTful API, running as a Node.js application on AWS EC2 node, using the Hyperledger Fabric Client SDK to query and invoke chaincode on AWS Managed Blockchain. [AWS Managed Blockchain](fabric-app/README.md)

This client does not communicate with the blockchain directly. It communicates to the AWS Lambda game server, which forwards the request to the blockchain when necessary.
