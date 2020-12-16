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

from math import sin, cos, sqrt, atan2, radians
from flask import jsonify, request

from models import DB
from controllers import bad_request_response, app

database = DB()


def find_distance(lat1, lon1, lat2, lon2):
    R = 6373.0
    lat1 = radians(float(lat1))
    lon1 = radians(float(lon1))
    lat2 = radians(float(lat2))
    lon2 = radians(float(lon2))

    dlon = lon2 - lon1
    dlat = lat2 - lat1

    a = sin(dlat / 2) ** 2 + cos(lat1) * cos(lat2) * sin(dlon / 2) ** 2
    c = 2 * atan2(sqrt(a), sqrt(1 - a))

    distance = R * c
    return distance


"""
    Creates a new hunt
"""


@app.route("/api/hunts", methods=["POST"])
def add_hunt():
    data = request.get_json()
    if data.get("name", None) is not None:
        response = database.create_hunt(data)
    if not response:
        return bad_request_response(message="Failed! Hunt was not created")
    return jsonify({"code": 200, "message": "Success", "data": response})


"""
    Get a hunt using Id
"""


@app.route("/api/hunt/<huntID>", methods=["GET"])
def get_hunt(huntID):
    response = database.get_hunt(huntID)
    if not response:
        return bad_request_response(message="Failed! Hunt not found")
    return jsonify({"code": 200, "message": "Success", "data": response["Item"]})


"""
    #Update hunt rating a hunt using Id
"""


@app.route("/api/hunt/<huntID>/rate", methods=["PUT"])
def update_rating(huntID):
    data = request.get_json()
    hunt_data = (database.get_hunt(huntID))["Item"]
    # Add the user rating to previous and divide with total users
    user_rating = data.get("user_rating", None)
    previous_user_rating = hunt_data.get("user_rating", None) * hunt_data.get(
        "num_raters", None
    )
    num_raters = hunt_data.get("num_raters", None) + 1
    new_user_rating = (user_rating + previous_user_rating) / (num_raters)
    update_data = {"user_rating": new_user_rating, "num_raters": num_raters}

    return_data = database.update_huntRating(huntID, update_data)

    if not return_data:
        return bad_request_response(message="Failed! Hunt not updated")
    return jsonify({"code": 200, "message": "Success", "data": return_data})


"""
    #Get a all clues using huntID
"""


@app.route("/api/hunt/<huntID>/clues", methods=["GET"])
def get_clues(huntID):
    response = database.get_hunt(huntID)
    if not response:
        return bad_request_response(message="Failed! Hunt not found")
    clues = response["Item"].get("clues", None)
    return jsonify({"code": 200, "message": "Success", "data": clues})


"""
    #Get a a specific clue using huntID and step_num
"""


@app.route("/api/hunt/<huntID>/clues/<int:step_num>", methods=["GET"])
def get_clue(huntID, step_num):
    response = database.get_hunt(huntID)
    if not response:
        return bad_request_response(message="Failed! Hunt not found")
    clues = response["Item"].get("clues", None)
    clue = clues[step_num]
    return jsonify({"code": 200, "message": "Success", "data": clue})


"""
    #Get a hints using huntID and clue number
"""


@app.route("/api/hunt/<huntID>/clues/<int:step_num>/hint", methods=["GET"])
def get_hint(huntID, step_num):
    response = database.get_hunt(huntID)
    if not response:
        return bad_request_response(message="Failed! Hunt not found")
    hints = response["Item"].get("hints", None)
    hint = hints[step_num]
    return jsonify({"code": 200, "message": "Success", "data": hint})


"""
    #Get a task using huntID  and instanceID
"""


@app.route("/api/hunt/<huntID>/task/<int:step_num>", methods=["GET"])
def get_task(huntID, step_num):
    hexID = request.args.get("instanceID")
    instanceID = int(hexID, 16)
    response = database.get_hunt(huntID)
    if not response:
        return bad_request_response(message="Failed! Hunt not found")
    tasks = response["Item"].get("tasks", None)
    task = tasks[step_num]
    if task.get("instanceID", None) == instanceID:
        return jsonify(
            {"code": 200, "message": "Success", "data": task.get("question", None)}
        )
    return bad_request_response(message="Wrong Location")


"""
    Nearby hunts
"""


@app.route("/api/<androidID>/hunt/nearby", methods=["GET"])
def nearby_hunt(androidID):
    latitude = str(request.args.get("latitude"))
    longitude = str(request.args.get("longitude"))
    radius = float(request.args.get("radius"))
    response = database.get_allHunt()
    print(response)
    player = (database.get_player(androidID)).get("Item", None)
    data = []
    if not response:
        return bad_request_response(message="Failed! Hunt not found")
    for item in response:
        distance = find_distance(
            latitude,
            longitude,
            str(item.get("start_lati", None)),
            str(item.get("start_long", None)),
        )
        if radius >= distance:
            have_started = False
            if (player.get("started_hunts")) is not None:
                for hunt in player.get("started_hunts"):
                    if hunt["huntID"] == item["huntID"]:
                        have_started = True
                        break
            if (player.get("completed_hunts")) is not None:
                for hunt in player.get("completed_hunts"):
                    if hunt["huntID"] == item["huntID"]:
                        have_started = True
                        break
            if not have_started:
                data.append(item)

    return jsonify({"code": 200, "message": "Success", "data": data})


"""
START A NEW HUNT
"""


@app.route("/api/<androidID>/<huntID>/start", methods=["PUT"])
def start_hunt(androidID, huntID):
    # data = request.get_json()
    response = database.get_hunt(huntID)
    if not response:
        return bad_request_response(message="Failed! Hunt not found")
    player = (database.get_player(androidID)).get("Item", None)
    have_started = False
    if (player.get("started_hunts")) is not None:
        for hunt in player["started_hunts"]:
            if hunt["huntID"] == huntID:
                have_started = True
                break
    if not have_started:
        return_data = database.startHunt(androidID, huntID)
    if not return_data:
        return bad_request_response(message="Failed! Hunt not updated")
    return jsonify({"code": 200, "message": "Success", "data": return_data})
