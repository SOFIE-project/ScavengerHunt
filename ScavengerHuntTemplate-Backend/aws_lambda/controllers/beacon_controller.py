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

from models import DB
from controllers import bad_request_response, app

database = DB()

"""
    Adds a new beacon
"""


@app.route("/api/beacon", methods=["POST"])
def add_beacon():
    data = request.get_json()
    if data.get("beacon_uuid", None) is not None:
        response = database.new_beacon(data)
    if not response:
        return bad_request_response(message="Failed! Beacon was not added")
    return jsonify({"code": 200, "message": "Success", "data": response})


@app.route("/api/beacon/<beaconID>", methods=["GET"])
def get_beacon(beaconID):
    response = database.get_beacon(beaconID)
    if not response:
        return bad_request_response(message="Failed! Beacon not found")
    return jsonify({"code": 200, "message": "Success", "data": response["Item"]})
