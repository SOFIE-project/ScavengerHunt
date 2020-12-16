# SOFIE Mobile Gaming Pilot 

**Table of contents:**

- [Description](#description)
    - [Flow of the game](#Flow-of-the-game)
- [Usage](#usage)
    - [Prerequisites](#prerequisites)
    - [Setting up the Scavenger Hunt client](#Setting-up-the-Scavenger-Hunt-client)
    - [Setting up the Blockmoji client](#Setting-up-the-Blockmoji-client)
    - [Setting up the backend](#Setting-up-the-backend)
    - [Setting up hunts and beacons](#Setting-up-hunts-and-beacons)
- [3rd party components](#3rd-party-components)
- [Known Issues](#Known-Issues)
- [Contact Info](#Contact-info)
- [License](#License)

## Description

This is an open-sourced version of Scavenger Hunt - a location-based game template, made as part of the [SOFIE](https://www.sofie-iot.eu/) collaborative EU project. This prototype uses Eddystone BLE beacons for positioning users in the real world, and mockup rewards are stored on a managed blockchain (Hyperledger Fabric). As the player plays the game, they receive Blockmoji item rewards that are stored on the Hyperledger Fabric network. Those same items can potentially be used across many games that share the same blockchain backend, and the player may perceive a sense of true ownerwhip of the items. A traditional game server is still used for most of the game logic.

Game Architecture 

<img src="/imgs/archi.png">

This prototype was created for the purpose of exploring use cases of Blockchain and IoT technologies in the context of mobile gaming. It was open-sourced to be used for educational and research purposes. We hope this template can serve as a useful starting point for anyone who wants to start exploring how a location-based game can be built while utilizing IoT beacons and certain features of a managed blockchain.

## Flow of the game

When the player opens the Scavenger Hunt game, they see which hunts are nearby based on their GPS location. After starting a hunt, the player sees a clue. It is a riddle that points them to the next physical location. When in the correct location, the player sees a question. By observing the physical surroundings and answering the question correctly, the player gets the next clue. After all clues in a hunt are completed, the player receives rewards: coins, gems, stars, and possibly virtual items. The player also gains XP and levels up their character. Leveling up does not currently provide any benefits, but progression does make completing hunts more satisfying and the reward window becomes more juicy.

Gems can be used to see hints for clues, and stars can be used to skip whole clues.

For more details:- [IEEE-Scavenger Hunt](https://ieeexplore.ieee.org/document/9253568)

## Usage

### Prerequisites

* Unity 2019.3 or higher (2019.1 or higher for Blockmoji) with Android / iOS build module.
* You will need BLE (Bluetooth low-energy) beacons that can use the Eddystone UID protocol.
* Android or iOS phone.
* Purchase your own iBeacon plugin from the Unity asset store. More detailed instructions below.

### Setting up the Scavenger Hunt client

The main game's Unity project (the client) is the ScavengerHuntTemplate-UnityClient folder. After you open the project in Unity for the first time, you will encounter compilation errors. This is expected. Many scripts depend on an "iBeacon" named plugin that is not included in this repository. In order to make the game work, you must purchase the iBeacon plugin from the Unity Asset Store and place the iBeacon folder under the Assets folder. After that, the game should compile.

Select the IBeacon object in the "MainScene" scene. If it does not have the scripts "IBeaconReceiver" and "BluetoothState" attached to it or if Unity complains about missing scripts, attach both of these scripts to that game object.

In addition, before the game is playable, the backend host address must be set. After setting up the backend as described in the next section, update the "_host_address" variable in the ServerHandler.cs file. Now the game can be built for a mobile phone and played if there are hunts on the backend (instructions for the backend below).

For testing purposes, getting nearby hunts also works in the Unity Editor on a machine without GPS capabilities. In that case, GPS latitude and longitude are hardcoded in function StartRefreshingHunts in the NearbyManager.cs script, which can be changed to your current coordinates.

When playing the game on mobile and completing hunts, your device ID will be used as the Player ID. If exiting the game and returning, your progress will be retained, and you will not be able to complete old hunts again. While ideal for a real life scenario, you might want to be able to replay hunts for testing purposes. For this, _newPlayerEveryTime in the Player.cs script can be set to true and a new account will be created every time you enter the game. Optionally, you can manually delete player items from the backend database.

This version of the game does not have any sound effects. Many buttons and UI elements have empty sound clip references that can be populated with custom sounds.

### Setting up the Blockmoji client

Blockmoji is a companion application where the user can browse item rewards that they have acquired from Scavenger Hunt. If you create another game that uses the Blockmoji standard and backend, items received from there will also be visible in this application. In the Blockmoji client, the user can see the items' attributes and the items can be equipped and unequipped from their avatar. A game that would use the Blockmoji framework could, if it wanted to, interpret these attributes to provide in-game benefits.

Once the Blockmoji backend has been set up, populate the _host_address field in the APIHandler.cs script.

### Setting up the backend

The Backend consists of the following components:

* RESTful API, running a python flask application on AWS LAMBDA and database on AWS DynamoDB, for the core game related tasks. [AWS LAMBDA](ScavengerHuntTemplate-Backend/aws_lambda/README.md)

* Fabric RESTful API, running as a Node.js application on AWS EC2 node, using the Hyperledger Fabric Client SDK to query and invoke chaincode on AWS Managed Blockchain. [AWS Managed Blockchain](ScavengerHuntTemplate-Backend/fabric-app/README.md)

This client does not communicate with the blockchain directly. It communicates to the AWS Lambda game server, which forwards the request to the blockchain when necessary.

Blockmoji backend must also be set up, as Scavenger Hunt depends on it (it awards Blockmoji items to the player).

### Setting up hunts and beacons

In Unity, in the Scavenger Hunt project, select the IBeacon object in the scene. In the inspector for the IBeaconReceiver script, create a new region with an arbitrary name (such as "com.test.ibeacon"), and specify that beacons are of type "Eddystone UID". Next, define a namespace for the beacons in 20 hexadecimal digits, which can be, for example "00000000000000000000".

We found the following detection parameters to work well: timespan = 6, scan period = 3 and between scan period = 0.

Next, let's set up a hunt! POST the content provided in exampleHunt.json to LAMBDA-API-URL/hunts, with a header "Content-Type" set to "application/json". In the hunt example, you can see that a clue points to a beacon with ID 17592186044416 as an integer, which translates to 100000000000 in hexadecimals. You can add new clues as you wish to the json that you post, as long as you increase "task_num" by 1 with each subsequent task, and accompany each new task with a new entry in "clues" and "hints".

Hunts may also have virtual item rewards. Before using this functionality, you should  create the items on the Blockmoji backend. To do this, POST a json to FABRIC-API-URL/item in the scheme defined by exampleItem.json (again, with the header "Content-Type" set to "application/json"). After the item exists on the backend, you can POST hunts that offer the item as a reward. Simply include the item's "UniqueID" string in the "assetRewards" list of the hunt. NOTE: For the game to work, at least one reward item (and a maximum of four) must be included in every hunt. If you accidentally POST an hunt without item rewards, you will have to delete that entry from the database.

To set up beacons, you can use a mobile app, such as "iBKS", to configure them. When configuring a beacon, make sure that they use the Eddystone UID protocol. In this protocol, each beacon has an UID which consists of 32 hexadecimal digits. The first 20 digits should be the namespace as defined in the Unity IBeaconReceiver script (as mentioned above). The rest 12 digits should be a unique identifier of the beacon. When creating hunts, you can, for simplicity, use small numbers as IDs, in order for them to be similar in appearance both as decimal integers and hexadecimals. For instance, if you configure a beacon's ID to be "000000000001", the integer "instanceID" in the hunt json should conveniently be 1.

## 3rd party components

* iBeacon Unity plugin for detecting BLE beacons (not included, user of this project has to purchase and download their own from the Unity Asset Store)
* Demigiant DOTween for easy UI animations (included as original source, not allowed to redistribute when modified)
* FrancoisOne, Luckiest Guy and Raleway Google Fonts
* Newtonsoft JSON.NET for parsing json into C# objects (MIT license)
* Fabric SDK for nodejs
* Zappa - Serverless python deployment on AWS

## Known Issues

* Node server with Fabric SDK on AWS EC2 instance sometimes stops responding, needs to be restarted.
* Keys managing identity on AWS managed Fabric blockchain are outdated after a few weeks, need to re-enrol the user to work.

## Contact Info

* **Max Samarin** - *Unity Client* 
* **Ahsan Manzoor** - *Backend*

Email: firstname.lastname@rovio.com

## License

This repository is licensed under the Apache License 2.0.

