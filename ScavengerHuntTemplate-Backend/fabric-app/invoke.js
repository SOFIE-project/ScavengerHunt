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
# permissions and limitations under the License.
*/

'use strict';
var util = require('util');
var helper = require('./connection.js');
var logger = helper.getLogger('Invoke');

var invokeChaincode = async function(peerNames, channelName, chaincodeName, args, fcn, username, orgName) {
        logger.info(util.format('\ninvokeChaincode - chaincode %s, function %s, on the channel \'%s\' for org: %s\n',
                chaincodeName, fcn, channelName, orgName));
        var error_message = null;
        var txIdAsString = null;
        try {
             	// first setup the client for this org
                var client = await helper.getClientForOrg(orgName, username);
                logger.info('invokeChaincode - Successfully got the fabric client for the organization "%s"', orgName);
                var channel = client.getChannel(channelName);
                if(!channel) {
                        let message = util.format('invokeChaincode - Channel %s was not defined in the connection profile', channelName);
                        logger.error(message);
                        throw new Error(message);
                }
                var txId = client.newTransactionID();
                txIdAsString = txId.getTransactionID();

                // send proposal to endorsing peers
                var request = {
                        targets: peerNames,
                        chaincodeId: chaincodeName,
                        fcn: fcn,
                        args: args,
                        chainId: channelName,
                        txId: txId
                };

                let results = await channel.sendTransactionProposal(request);
                var proposalResponses = results[0];
                var proposal = results[1];
                logger.info('ponse %s', results);
                var successfulResponses = true;
                for (var i in proposalResponses) {
                        let oneSuccessfulResponse = false;
                        if (proposalResponses && proposalResponses[i].response &&
                                proposalResponses[i].response.status === 200) {
                                oneSuccessfulResponse = true;
                                logger.info('invokeChaincode - received successful proposal response');
                        } else {
                                logger.error('invokeChaincode - received unsuccessful proposal response');
                        }
                        successfulResponses = successfulResponses & oneSuccessfulResponse;
                }

                if (successfulResponses) {
                        var promises = [];
                        let event_hubs = channel.getChannelEventHubsForOrg();
                        event_hubs.forEach((eh) => {
                                let invokeEventPromise = new Promise((resolve, reject) => {
                                        let event_timeout = setTimeout(() => {
                                                let message = 'REQUEST_TIMEOUT:' + eh.getPeerAddr();
                                                logger.error(message);
                                                eh.disconnect();
                                        }, 3000);
                                        eh.registerTxEvent(txIdAsString, (tx, code, block_num) => {
                                                logger.info('invokeChaincode - The invoke chaincode transaction has been committed on peer %s',eh.getPeerAddr());
                                                clearTimeout(event_timeout);

                                                if (code !== 'VALID') {
                                                        let message = util.format('invokeChaincode - The invoke chaincode transaction was invalid, code:%s',code);
                                                        logger.error(message);
                                                        reject(new Error(message));
                                                } else {
                                                        let message = 'invokeChaincode - The invoke chaincode transaction was valid.';
                                                        logger.info(message);
                                                        resolve(message);
                                                }
                                        }, (err) => {
                                                clearTimeout(event_timeout);
                                                logger.error(err);
                                                reject(err);
                                        },
                                                {unregister: true, disconnect: true}
                                        );
                                        eh.connect();
                                });
                                promises.push(invokeEventPromise);
                        });

                        var orderer_request = {
                                txId: txId,
                                proposalResponses: proposalResponses,
                                proposal: proposal
                        };
                        var sendPromise = channel.sendTransaction(orderer_request);
                        promises.push(sendPromise);
                        let results = await Promise.all(promises);
                        logger.info(util.format('invokeChaincode ----->>> R E S P O N S E : %j', results));
                        let response = results.pop(); //  ordering service results are last in the results
                        if (response.status === 'SUCCESS') {
                                logger.info('invokeChaincode - Successfully sent transaction to the ordering service.');
                        } else {
                                error_message = util.format('invokeChaincode - Failed to order the transaction. Error code: %s',response.status);
                                logger.info(error_message);
                        }

                        // See what each of the event hubs reported
                        for(let i in results) {
                                let event_hub_result = results[i];
                                let event_hub = event_hubs[i];
                                logger.info('invokeChaincode - Event results for event hub :%s',event_hub.getPeerAddr());
                                if(typeof event_hub_result === 'string') {
                                        logger.info('invokeChaincode - ' + event_hub_result);
                                } 
                                else {
                                      	if (!error_message) error_message = event_hub_result.toString();
                                        logger.info('invokeChaincode - ' + event_hub_result.toString());
                                }
                        }
                }
                else {
                      	error_message = util.format('invokeChaincode - Failed to send Proposal and receive all good ProposalResponse. Status code: ' +
                                proposalResponses[0].status + ', ' + 
                                proposalResponses[0].message + '\n' +  
                                proposalResponses[0].stack);
                        logger.info(error_message);
                }
        } 
	catch (error) {
                logger.error('invokeChaincode - Failed to invoke due to error: ' + error.stack ? error.stack : error);
                error_message = error.toString();
        }
        if (!error_message) {
                let message = util.format(
                        'invokeChaincode - Successfully invoked chaincode %s, function %s, on the channel \'%s\' for org: %s and transaction ID: %s',
                        chaincodeName, fcn, channelName, orgName, txIdAsString);
                logger.info(message);
                let response = {};
                response.transactionId = txIdAsString;
                return response;
        } 
	else {
              	let message = util.format('invokeChaincode - Failed to invoke chaincode. cause:%s', error_message);
                logger.error(message);
                throw new Error(message);
        }
};

exports.invokeChaincode = invokeChaincode;











