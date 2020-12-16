# AWS Lambda and DynamoDB

**NOTE:- Please configure the Fabric network before deploying or using this template as some API access blockchain for data**

This repo includes RESTAPIs for the core functionality of the SOFIE mobile gaming pilot. 

Please follow the steps to deploy the functions on AWS Lambda with DynamoDB:

## Step 1 - Create the credentials file

Create a file called credentials and store the aws_access_key_id and aws_secret_access_key. The other specifics like default_region can also be stored.

You can also use AWS cli to set the aws_access_key_id and aws_secret_access_key. To install aws_cli,

```
pip install awscli
```

## Step 2 - Create Virtual Environment

Change the working directory to flask-app and create a virtual environment first

```
cd aws_lambda
virtualenv .env
source .env/bin/activate
```

## Step 3 - Install required packages

Install the required packages using PIP

```
pip install -r requirements.txt
```

## Step 4 - Create the database on AWS DynamoDB

AWS Cli must be configured with crediantials before creating the database

```
cd models/schemas
./create_tables.sh
```

## Step 5 - Deploy this applcation onto Amazon lambda.

After running zappa init command it will create a file called zappa_settings.json will helps in deploying your application.

```
zappa init
zappa deploy dev
```
