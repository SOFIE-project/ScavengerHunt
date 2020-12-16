/*
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
 permissions and limitations under the License.
*/

/* Hyperledger Fabric connection file for Scavenger
*/
'use strict';
var log4js = require('log4js');
var logger = log4js.getLogger('Connection');
var util = require('util');
var hfc = require('fabric-client');
hfc.setLogger(logger);

async function getClientForOrg (userorg, username) {
    logger.info('START getClientForOrg for org %s and user %s', userorg, username);
    let config = './networkConnection.yaml';
    let clientConfig = './client.yaml';
    let client = hfc.loadFromConfig(config);
    client.loadFromConfig(clientConfig);

    await client.initCredentialStores();

    if(username) {
        let user = await client.getUserContext(username, true);
        if(!user) {
                throw new Error(util.format('getClient - User was not found :', username));
            } else{
                logger.info('getClient - User %s was found to be registered and enrolled', username);
            }
        }
	logger.info('END getClientForOrg for org %s and user %s \n\n', userorg, username);

    return client;
}

var getRegisteredUser = async function(username, userorg, isJson) {
        try {
             	logger.info('START getRegisteredUser - for org %s and user %s', userorg, username);
                var client = await getClientForOrg(userorg);
                var user = await client.getUserContext(username, true);
                if (user && user.isEnrolled()) {
                        logger.info('getRegisteredUser - User %s already enrolled', username);
                } else {
                        // user was not enrolled, so we will need an admin user object to register
                        logger.info('getRegisteredUser - User %s was not enrolled, so we will need an admin user object to register', username);
                        var admins = hfc.getConfigSetting('admins');;
                        let adminUserObj = await client.setUserContext({username: admins[0].username, password: admins[0].secret});
                        let caClient = client.getCertificateAuthority();
                        let secret = await caClient.register({
                                enrollmentID: username
                        }, adminUserObj);
                        user = await client.setUserContext({username:username, password:secret});
                        logger.info('getRegisteredUser - Successfully enrolled username %s  and setUserContext on the client object', username);
                }
		if(user && user.isEnrolled) {
                        if (isJson && isJson === true) {
                                var response = {
                                        success: true,
                                        secret: user._enrollmentSecret,
                                        message: username + ' enrolled Successfully',
                                };
                                return response;
                        }
                    }
		else{
				throw new Error('getRegisteredUser - User was not enrolled ');
		}
	}
	catch(error) 
	{
		logger.error('getRegisteredUser - Failed to get registered user: %s with error: %s', username, error.toString());
		return 'failed '+error.toString();
    }
};

var getLogger = function(moduleName) {
        var logger = log4js.getLogger(moduleName);
        return logger;
};

exports.getClientForOrg = getClientForOrg;
exports.getRegisteredUser = getRegisteredUser;
exports.getLogger = getLogger;


