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

from flask import jsonify, request
import requests
import socket

from models import DB
from controllers import bad_request_response, app, fabric_server


database = DB()
HEADER = {"Content-Type": "application/json"}
WORDS_REMOVED = ["the", "an", "a"]

"""
    Creates a new player
"""


@app.route("/api/players", methods=["POST"])
def register_player():
    data = request.get_json()
    if data.get("androidId", None) is not None:
        playerData = database.get_player(data.get("androidId", None))
        if playerData.get("Item", None) is None:
            response = database.create_player(data)
            try:
                """Create HF account"""
                PARAMS = {"username": data.get("androidId", None)}
                r = requests.post(url=fabric_server + "/users", json=PARAMS, headers=HEADER)
                content = r.json()
            except socket.timeout:
                return bad_request_response(message="Failed! Problem in creating fabric account")
            if not response:
                return bad_request_response(message="Failed! User was not created")
    return jsonify({"code": 200, "message": "Success", "data": content})


"""
    Get a player details
"""


@app.route("/api/player/<androidId>", methods=["GET"])
def get_player(androidId):
    response = database.get_player(androidId)
    if not response:
        return bad_request_response(message="Failed! Hunt not found")
    return jsonify({"code": 200, "message": "Success", "data": response["Item"]})


"""
    #Get  a Started player hunt
"""


@app.route("/api/player/<androidId>/startedHunts", methods=["GET"])
def get_started_hunt(androidId):
    response = database.get_player(androidId)
    if not response:
        return bad_request_response(message="Failed! Hunt not found")
    reply = []
    if response["Item"].get("started_hunts", None) is not None:
        for hunt in response["Item"].get("started_hunts", None):
            print(response["Item"])
            hunt_info = database.get_hunt(hunt["huntID"])
            reply_data = {"started_hunt": hunt, "hunt_info": hunt_info["Item"]}
            reply.append(reply_data)
    return jsonify({"code": 200, "message": "Success", "data": reply})


"""
    #Get  a completed player hunt
"""


@app.route("/api/player/<androidId>/completedHunts", methods=["GET"])
def get_completed_hunt(androidId):
    response = database.get_player(androidId)
    if not response:
        return bad_request_response(message="Failed! Hunt not found")
    reply = []
    for hunt in response["Item"].get("completed_hunts", None):
        hunt_info = database.get_hunt(hunt["huntID"])
        reply_data = {"completed_hunt": hunt, "hunt_info": hunt_info["Item"]}
        reply.append(reply_data)
    return jsonify({"code": 200, "message": "Success", "data": reply})


"""
    #Use star
"""


@app.route("/api/player/<androidId>/<huntID>/star", methods=["PUT"])
def use_star(androidId, huntID):
    # data = request.get_json()
    playerData = (database.get_player(androidId))["Item"]
    if playerData["stars"] > 0:
        for hunt in playerData["started_hunts"]:
            if huntID == hunt["huntID"] and hunt["hasUsedStar"]:
                stars = playerData["stars"] - 1
                response = database.update_star(
                    huntID, playerData, stars, (hunt["current_clue"] + 1)
                )
                if not response:
                    return bad_request_response(message="Failed! Hunt not found")
                return jsonify({"code": 200, "message": "Success", "data": response})
        return bad_request_response(message="Has used star for this Hunt")
    return bad_request_response(message="Not Enough Stars")


"""
    #Check for the answer
"""


@app.route("/api/player/<androidId>/<huntID>/<int:step_num>/answer", methods=["PUT"])
def check_answer(androidId, huntID, step_num):
    data = request.get_json()
    response = database.get_hunt(huntID)
    if not response:
        return bad_request_response(message="Failed! Hunt not found")
    right_answer = (response["Item"].get("tasks", None))[step_num].get("answer")
    numClue = len(response["Item"]["clues"])
    playerData = database.get_player(androidId)["Item"]
    user_answer = data["answer"]
    for letter in WORDS_REMOVED:
        right_answer = (right_answer.lower()).replace(letter, "")
        user_answer = (user_answer.lower()).replace(letter, "")
    right_answer = right_answer.replace(" ", "")
    user_answer = user_answer.replace(" ", "")
    if user_answer == right_answer:
        for hunt in playerData["started_hunts"]:
            if hunt["huntID"] == huntID and step_num == (numClue - 1):
                database.moveHunt(androidId, huntID)
                response = database.updateReward(androidId, huntID)
                return jsonify({"code": 200, "message": "Success", "data": response})
        response = database.update_playerHunt(androidId, huntID, (step_num + 1))
        return jsonify({"code": 200, "message": "Success", "data": response})
    return bad_request_response(message="Incorrect answer")


"""
    #Check for the star used
"""


@app.route("/api/player/<androidId>/<huntID>/star", methods=["GET"])
def check_star(androidId, huntID):
    # data = request.get_json()
    playerData = (database.get_player(androidId))["Item"]
    for hunt in playerData["started_hunts"]:
        if huntID == hunt["huntID"]:
            response = hunt["hasUsedStar"]
            if response is None:
                return bad_request_response(message="Failed! Hunt not found")
    return jsonify({"code": 200, "message": "Success", "data": response})


"""
    #Check for assets
"""


@app.route("/api/player/<androidId>/myassets", methods=["GET"])
def get_assets(androidId):
    # data = request.get_json()
    playerData = (database.get_player(androidId))["Item"]
    response = playerData["assets"]
    if response is None:
        return bad_request_response(message="Failed! Hunt not found")
    return jsonify({"code": 200, "message": "Success", "data": response})


"""
    #Use a hints using huntID and clue number
"""


@app.route(
    "/api/player/<androidId>/<huntID>/clues/<int:step_num>/hint", methods=["PUT"]
)
def use_hint(androidId, huntID, step_num):
    # data = request.get_json()
    playerData = (database.get_player(androidId))["Item"]
    response = database.get_hunt(huntID)
    if not response:
        return bad_request_response(message="Failed! Hunt not found")
    hint = None
    if playerData["gems"] >= 10:
        hints = response["Item"].get("hints", None)
        hint = hints[step_num]
        response = database.update_gems(playerData)
    return jsonify({"code": 200, "message": "Success", "data": hint})


"""
    #cheat to add reward
"""


@app.route("/api/player/<androidId>/addreward", methods=["GET"])
def add_reward(androidId):
    # data = request.get_json()
    response = database.cheatReward(androidId)
    return jsonify({"code": 200, "message": "Success", "data": response})


"""
    Use offer
"""


@app.route("/api/player/<androidId>/shop/<offerId>", methods=["PUT"])
def use_offer(androidId, offerId):
    # data = request.get_json()
    playerData = (database.get_player(androidId))["Item"]
    offerData = (database.get_offer(offerId))["Item"]
    if playerData["coins"] >= offerData["coins"]:
        response = database.use_offer(androidId, offerId)
        if not response:
            return bad_request_response(message="Failed! Player not found")
        return jsonify({"code": 200, "message": "Success", "data": response})
    return bad_request_response(message="Not Enough Coins to Buy")
