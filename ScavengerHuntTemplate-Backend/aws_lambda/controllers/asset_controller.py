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
    Creates a new asset
"""


@app.route("/api/assets", methods=["POST"])
def create_asset():
    data = request.get_json()
    if data.get("assetName", None) is not None:
        response = database.create_asset(data)
    if not response:
        return bad_request_response(message="Failed! Asset was not created")
    return jsonify({"code": 200, "message": "Success", "data": response})


"""
    Get a asset using Id
"""


@app.route("/api/asset/<assetId>", methods=["GET"])
def get_asset(assetId):
    response = database.get_asset(assetId)
    if not response:
        return bad_request_response(message="Failed! Asset not found")
    return jsonify({"code": 200, "message": "Success", "data": response["Item"]})


"""
    Get all assets
"""


@app.route("/api/assets", methods=["GET"])
def get_allassets():
    response = database.get_assets()
    if not response:
        return bad_request_response(message="Failed! Assets not found")
    return jsonify({"code": 200, "message": "Success", "data": response["Items"]})
