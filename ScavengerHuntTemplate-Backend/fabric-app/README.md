# Hyperledger Fabric Blockchain with APIs

This repo includes backend for interacting with the Hyperledger Fabric blockchain. In the SOFIE mobile gaming, AWS managed blockchain was used as the Fabric ledger. This repo can also be configured with any Hyperleder Fabric v1.2 network deployment. This repo is dependent on Hyperleder Fabric SDK for nodejs and provides API for client applications to interact with smart contracts deployed to Fabric network.

Please follow the steps to create a Fabric blockchain network on AWS.

## Step 1 - Create the Hyperledger Fabric blockchain network

Create a 'Starter' Fabric network with two small peer nodes on AWS Managed Blockchain.

## Step 2 - Check the network is AVAILABLE

Before continuing, check to see that your Fabric network has been created and is Available. It does take quite a while
to create the network.

## Step 3 - Create the Fabric client node

Create the Fabric client node as a EC2 node on AWS, which will host the Fabric CLI. You will use the CLI to administer the Fabric network.

## Step 4 - Prepare the Fabric client node and enroll an identity

Prior to executing any commands on the Fabric client node, you will need to export ENV variables that provide a context to Hyperledger Fabric. 

SSH into the Fabric client node.

Clone this repo and edit the fabric-export.sh file with AWS managed blockchain details.

```
cd node-app
source fabric-exports.sh
```

Sourcing the file will export the necessary ENV variables

Get the latest version of the Managed Blockchain PEM file. This will overwrite the existing file in the home directory with the latest version of this file:

```
aws s3 cp s3://us-east-1.managedblockchain/etc/managedblockchain-tls-chain.pem  /home/ec2-user/managedblockchain-tls-chain.pem
```

Enroll an admin identity with the Fabric CA (certificate authority). We will use this identity to administer the Fabric network and perform tasks such as creating channels and instantiating chaincode.

```
cd ~
fabric-ca-client enroll -u https://$ADMINUSER:$ADMINPWD@$CASERVICEENDPOINT --tls.certfiles /home/ec2-user/managedblockchain-tls-chain.pem -M /home/ec2-user/admin-msp 
```

Some final copying of the certificates is necessary:

```
mkdir -p /home/ec2-user/admin-msp/admincerts
cp ~/admin-msp/signcerts/* ~/admin-msp/admincerts/
```

## Step 5 - Update the configtx channel configuration
On the Fabric client node.

Update the configtx channel configuration. The Name and ID fields should be updated with the member ID from AWS Managed Blockchain.

```
cp ~/node-app/configtx.yaml ~
sed -i "s|__MEMBERID__|$MEMBERID|g" ~/configtx.yaml
```

Generate the configtx channel configuration by executing the following script. When the channel is created, this channel configuration will become the genesis block (i.e. block 0) on the channel:

```
docker exec cli configtxgen -outputCreateChannelTx /opt/home/$CHANNEL.pb -profile OneOrgChannel -channelID $CHANNEL --configPath /opt/home/
```

Check that the channel configuration has been generated:

```
ls -lt ~/$CHANNEL.pb 
```

## Step 6 - Create a Fabric channel

Create a Fabric channel.

Execute the following script:

```
docker exec -e "CORE_PEER_TLS_ENABLED=true" -e "CORE_PEER_TLS_ROOTCERT_FILE=/opt/home/managedblockchain-tls-chain.pem" \
    -e "CORE_PEER_ADDRESS=$PEER" -e "CORE_PEER_LOCALMSPID=$MSP" -e "CORE_PEER_MSPCONFIGPATH=$MSP_PATH" \
    cli peer channel create -c $CHANNEL -f /opt/home/$CHANNEL.pb -o $ORDERER --cafile $CAFILE --tls --timeout 900s
```

## Step 7 - Join your peer node to the channel

Join peer to Fabric channel.

Execute the following script:

```
docker exec -e "CORE_PEER_TLS_ENABLED=true" -e "CORE_PEER_TLS_ROOTCERT_FILE=/opt/home/managedblockchain-tls-chain.pem" \
    -e "CORE_PEER_ADDRESS=$PEER" -e "CORE_PEER_LOCALMSPID=$MSP" -e "CORE_PEER_MSPCONFIGPATH=$MSP_PATH" \
    cli peer channel join -b $CHANNEL.block  -o $ORDERER --cafile $CAFILE --tls
```

## Step 8 - Install chaincode on your peer node
Install chaincode on Fabric peer.

Execute the following script:

```
docker exec -e "CORE_PEER_TLS_ENABLED=true" -e "CORE_PEER_TLS_ROOTCERT_FILE=/opt/home/managedblockchain-tls-chain.pem" \
    -e "CORE_PEER_ADDRESS=$PEER" -e "CORE_PEER_LOCALMSPID=$MSP" -e "CORE_PEER_MSPCONFIGPATH=$MSP_PATH" \
    cli peer chaincode install -n $CHAINCODENAME -v $CHAINCODEVERSION -p $CHAINCODEDIR
```
## Step 9 - Install Node
On the Fabric client node.

Amazon Linux seems to be missing g++, so:

```
sudo yum install gcc-c++ -y
```

Install Node.js. We use v14.x.

```
curl -o- https://raw.githubusercontent.com/creationix/nvm/v0.35.3/install.sh | bash
```

```
. ~/.nvm/nvm.sh
nvm install 14
nvm use 14
```

## Step 10 - Install dependencies
On the Fabric client node.

```
cd ~/node-app
npm install
```

## Step 11 - Generate a connection profile

The REST API needs a connection profile to connect to the Fabric network. Connection profiles describe
the Fabric network and provide information needed by the Node.js application in order to connect to the
Fabric network. Edit the 'connectionprofile.yaml' and check that the connection profile contains 
URL endpoints for the peer, ordering service and CA, an 'mspid', a 'caName', and that the admin username and password
match those you entered when creating the Fabric network. 

## Step 12 - Run the REST API Node.js application

Run the app:

```
cd ~/node-app
nvm use 14
node app.js 
```
