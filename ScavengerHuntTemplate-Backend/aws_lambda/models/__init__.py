"""
# Licensed to the Apache Software Foundation (ASF) under one or more
# contributor license agreements.  See the NOTICE file distributed
# with this work for additional information regarding copyright
# ownership.  The ASF licenses this file to you under the Apache
# License, Version 2.0 (the "License"); you may not use this file
# except in compliance with the License.  You may obtain a copy of the
# License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
# implied.  See the License for the specific language governing
# permissions and limitations under the License.
"""

from decimal import Decimal
import uuid
from datetime import datetime
import requests

import boto3
from botocore.exceptions import ClientError

fabric_server = "{FABRIC NODE SERVER}"
HEADER = {"Content-Type": "application/json"}


class DB(object):
    def __init__(self):
        super(DB, self).__init__()
        self.dynamodb = boto3.resource("dynamodb")

    """
        IoT BEACONS
    """

    def new_beacon(self, data):
        table = self.dynamodb.Table("iotbeacon")
        user = {
            "ID": str(uuid.uuid1()),
            "beacon_uuid": data.get("beacon_uuid", None),
            "name": data.get("name", None),
            "latitude": data.get("latitude", None),
            "longitude": data.get("longitude", None),
            "major": data.get("major", None),
            "minor": data.get("minor", None),
            "created_at": str(datetime.utcnow()),
        }
        try:
            response = table.put_item(Item=user, ReturnValues="ALL_OLD")
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    """
        HUNTS
    """

    def create_hunt(self, data):
        table = self.dynamodb.Table("hunts")
        hunt = {
            "huntID": str(uuid.uuid1()),
            "name": data.get("name", None),
            "creator": data.get("creator", None),
            "description": data.get("description", None),
            "start_long": data.get("start_long", None),
            "start_lati": data.get("start_lati", None),
            "difficulty": Decimal(data.get("difficulty", None)),
            "user_rating": Decimal(data.get("user_rating", None)),
            "num_raters": data.get("num_raters", None),
            "publish_time": data.get("publish_time", None),
            "expiration_time": data.get("expiration_time", None),
            "rewardcoinspool": data.get("rewardcoinspool", None),
            "fixedcoinreward": data.get("fixedcoinreward", None),
            "assetRewards": data.get("assetRewards", None),
            "maxtotalfixedpool": data.get("maxtotalfixedpool", None),
            "clues": data.get("clues", None),
            "tasks": data.get("tasks", None),
            "hints": data.get("hints", None),
            "message": data.get("message", None),
            "created_at": str(datetime.utcnow()),
        }
        try:
            response = table.put_item(Item=hunt)
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def get_hunt(self, huntID):
        table = self.dynamodb.Table("hunts")
        try:
            response = table.get_item(Key={"huntID": huntID})

            """ ADD THE DETAIL OF ASSET REWARDS FOR SCAVENGER"""
            r = requests.get(
                fabric_server + "item/" + response["Item"]["assetRewards"][0]
            )
            response["Item"]["assetRewards"][0] = r.json()[0]
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def update_huntRating(self, huntID, data):
        table = self.dynamodb.Table("hunts")
        hunt = {
            ":r": Decimal(data.get("user_rating", None)),
            ":n": data.get("num_raters", None),
        }
        update = "set user_rating = :r, num_raters=:n"
        try:
            response = table.update_item(
                Key={"huntID": huntID},
                UpdateExpression=update,
                ExpressionAttributeValues=hunt,
            )
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def get_allHunt(self):
        reply = []
        table = self.dynamodb.Table("hunts")
        try:
            response = table.scan()
            for item in response["Items"]:
                """ ADD THE DETAIL OF ASSET REWARDS FOR SCAVENGER"""
                r = requests.get(fabric_server + "item/" + item["assetRewards"][0])
                item["assetRewards"][0] = r.json()[0]
                reply.append(item)
            return reply
        except ClientError as e:
            print("Unexpected error: %s" % e)

    """
        PLAYERS
    """

    def create_player(self, data):
        table = self.dynamodb.Table("players")
        player = {
            "androidId": data.get("androidId", None),
            "coins": data.get("coins", 0),
            "stars": data.get("stars", 0),
            "gems": data.get("gems", 0),
            "assets": data.get("assets", []),
            "total_xp": data.get("total_xp", 0),
            "started_hunts": data.get("started_hunts", []),
            "completed_hunts": data.get("completed_hunts", []),
            "created_at": str(datetime.utcnow()),
        }
        try:
            response = table.put_item(Item=player)
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def get_player(self, androidId):
        table = self.dynamodb.Table("players")
        try:
            response = table.get_item(Key={"androidId": androidId})
            """
            From HF
            """
            if response.get("Item", None) is not None:
                r = requests.get(url=fabric_server + "supply/" + androidId)
                response["Item"]["coins"] = r.json().get("coins", None)
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def update_playerHunt(self, playerId, huntID, data):
        table = self.dynamodb.Table("players")
        playerData = self.get_player(playerId)["Item"]
        i = 0
        for hunt in playerData["started_hunts"]:
            if hunt["huntID"] == huntID:
                update = "set started_hunts[" + str(i) + "].current_clue = :c"
            i = i + 1
        player = {":c": data}
        try:
            response = table.update_item(
                Key={"androidId": str(playerId)},
                UpdateExpression=update,
                ExpressionAttributeValues=player,
            )
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def update_star(self, huntID, playerData, stars, current_clue):
        table = self.dynamodb.Table("players")
        player = {":s": stars, ":c": current_clue, ":h": True}
        i = 0
        for hunt in playerData["started_hunts"]:
            if hunt["huntID"] == huntID:
                update = (
                    "set started_hunts["
                    + str(i)
                    + "].current_clue = :c, stars = :s, started_hunts["
                    + str(i)
                    + "].hasUsedStar = :h"
                )
            i = i + 1
        try:
            response = table.update_item(
                Key={"androidId": playerData["androidId"]},
                UpdateExpression=update,
                ExpressionAttributeValues=player,
            )
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def moveHunt(self, playerId, huntID):
        table = self.dynamodb.Table("players")
        playerData = self.get_player(playerId)["Item"]
        start = []
        complete = []
        for hunt in playerData["started_hunts"]:
            if hunt["huntID"] != huntID:
                start.append(hunt)
            elif hunt["huntID"] == huntID:
                complete.append(hunt)
        player = {":c": complete, ":s": start}
        update = "set started_hunts = :s, completed_hunts = :c"
        try:
            response = table.update_item(
                Key={"androidId": str(playerId)},
                UpdateExpression=update,
                ExpressionAttributeValues=player,
            )
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def updateReward(self, playerId, huntID):
        tablePlayer = self.dynamodb.Table("players")
        tableHunt = self.dynamodb.Table("hunts")
        playerData = self.get_player(playerId)["Item"]
        huntData = self.get_hunt(huntID)["Item"]
        player = {
            ":c": (huntData["fixedcoinreward"] + playerData["coins"]),
            ":g": (playerData["gems"] + 10),
            ":s": (playerData["stars"] + 1),
            ":x": (playerData["total_xp"] + 150),
        }
        updatePlayer = "set coins = :c, stars = :s, gems = :g, total_xp = :x"
        hunt = {":r": (huntData["rewardcoinspool"] - huntData["fixedcoinreward"])}
        updateHunt = "set rewardcoinspool = :r"
        reward = {
            "coins_reward": huntData["fixedcoinreward"],
            "gems_reward": 10,
            "stars_reward": 1,
            "asset_reward": huntData["assetRewards"][0],
            "total_xp": 150,
            "message": huntData["message"],
        }
        try:
            tablePlayer.update_item(
                Key={"androidId": str(playerId)},
                UpdateExpression=updatePlayer,
                ExpressionAttributeValues=player,
            )
            tableHunt.update_item(
                Key={"huntID": huntID},
                UpdateExpression=updateHunt,
                ExpressionAttributeValues=hunt,
            )
            """
            transfer coins
            """
            PARAMS = {
                "from": "TotalSupply",
                "to": playerId,
                "ammount": str(huntData["fixedcoinreward"]),
            }
            r = requests.post(
                url=fabric_server + "transfer/", json=PARAMS, headers=HEADER
            )

            """
            ADD asset to his BC blockmoji
            """
            updateItemsList = []
            r = requests.get(url=fabric_server + "useritems/" + playerId)
            ownedItems = r.json()[0].get("ownedItems", None)
            for item in ownedItems:
                updateItemsList.append(item.get("UniqueID", None))
            updateItemsList.append(huntData["assetRewards"][0]["UniqueID"])
            PARAMS = {"username": playerId, "OwnedItems": updateItemsList}
            requests.post(
                url=fabric_server + "updateOwn", json=PARAMS, headers=HEADER
            )

            return reward
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def startHunt(self, androidId, huntID):
        table = self.dynamodb.Table("players")
        start = {
            "huntID": huntID,
            "start_time": str(datetime.utcnow()),
            "end_time": None,
            "current_clue": 0,
            "hasUsedStar": False,
        }
        player = {":i": [start]}
        # update="set started_hunts = :s, completed_hunts = :c"
        update = "SET started_hunts = list_append(started_hunts, :i)"
        try:
            response = table.update_item(
                Key={"androidId": str(androidId)},
                UpdateExpression=update,
                ExpressionAttributeValues=player,
            )
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def update_gems(self, playerData):
        table = self.dynamodb.Table("players")
        player = {":g": (playerData["gems"] - 10)}
        update = "set gems = :g"
        try:
            response = table.update_item(
                Key={"androidId": playerData["androidId"]},
                UpdateExpression=update,
                ExpressionAttributeValues=player,
            )
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def cheatReward(self, androidId):
        tablePlayer = self.dynamodb.Table("players")
        player = {":c": 1000, ":g": 10, ":s": 1}
        updatePlayer = "set coins = :c, stars = :s, gems = :g"
        try:
            tablePlayer.update_item(
                Key={"androidId": str(androidId)},
                UpdateExpression=updatePlayer,
                ExpressionAttributeValues=player,
            )
            return player
        except ClientError as e:
            print("Unexpected error: %s" % e)

    """
        USERS
    """

    def create_users(self, data):
        table = self.dynamodb.Table("users")
        user = {
            "userId": str(uuid.uuid1()),
            "name": data.get("name", None),
            "email": data.get("email", None),
            "make_hunt": data.get("make_hunt", None),
            "role": data.get("role", None),
            "created_at": str(datetime.utcnow()),
        }
        try:
            response = table.put_item(Item=user, ReturnValues="ALL_OLD")
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    """
        ASSETS
    """

    def create_asset(self, data):
        table = self.dynamodb.Table("assets")
        asset = {
            "assetId": str(uuid.uuid1()),
            "assetName": data.get("assetName", None),
            "description": data.get("description", None),
            "imageURL": data.get("imageURL", None),
            "created_at": str(datetime.utcnow()),
        }
        try:
            response = table.put_item(Item=asset, ReturnValues="ALL_OLD")
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def get_asset(self, assetId):
        table = self.dynamodb.Table("assets")
        try:
            response = table.get_item(Key={"assetId": assetId})
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def get_assets(self):
        table = self.dynamodb.Table("assets")
        try:
            response = table.scan()
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    """
        OFFERS
    """

    def create_offer(self, data):
        table = self.dynamodb.Table("offer")
        offer = {
            "offerId": str(uuid.uuid1()),
            "coins": data.get("coins", None),
            "stars": data.get("stars", None),
            "gems": data.get("gems", None),
            "created_at": str(datetime.utcnow()),
        }
        try:
            response = table.put_item(Item=offer, ReturnValues="ALL_OLD")
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def get_offer(self, offerId):
        table = self.dynamodb.Table("offer")
        try:
            response = table.get_item(Key={"offerId": offerId})
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def get_offers(self):
        table = self.dynamodb.Table("offer")
        try:
            response = table.scan()
            return response
        except ClientError as e:
            print("Unexpected error: %s" % e)

    def use_offer(self, androidId, offerId):
        tablePlayer = self.dynamodb.Table("players")
        playerData = self.get_player(androidId)["Item"]
        offerData = self.get_offer(offerId)["Item"]
        player = {
            ":c": playerData["coins"] - offerData["coins"],
            ":g": playerData["gems"] + offerData["gems"],
            ":s": playerData["stars"] + offerData["stars"],
        }
        updatePlayer = "set coins = :c, stars = :s, gems = :g"
        try:
            tablePlayer.update_item(
                Key={"androidId": str(androidId)},
                UpdateExpression=updatePlayer,
                ExpressionAttributeValues=player,
            )
            return player
        except ClientError as e:
            print("Unexpected error: %s" % e)
