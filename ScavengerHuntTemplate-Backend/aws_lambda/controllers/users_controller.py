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

"""
    Creates a new user
"""


@app.route("/api/ping")
def index():
    """ static files serve """
    # return send_from_directory('dist', 'index.html')
    return jsonify({"message": "Success", "data": "Pong"})


@app.route("/api/users", methods=["POST"])
def register_user():
    data = request.get_json()
    database = DB()
    if data.get("name", None) is not None and data.get("email", None) is not None:
        response = database.create_users(data)
    if not response:
        return bad_request_response(message="Failed! User was not created")
    return jsonify({"code": 200, "message": "Success", "data": response})
