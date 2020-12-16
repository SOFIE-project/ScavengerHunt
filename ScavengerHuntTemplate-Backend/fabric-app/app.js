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

'use strict';
var log4js = require('log4js');
log4js.configure({
        appenders: {
          out: { type: 'stdout' },
        },
	categories: {
          default: { appenders: ['out'], level: 'info' },
        }
});
var logger = log4js.getLogger('API');
const WebSocketServer = require('ws');
var express = require('express');
var bodyParser = require('body-parser');
var http = require('http');
var util = require('util');
var app = express();
var cors = require('cors');
var hfc = require('fabric-client');
const uuidv4 = require('uuid/v4');

var connection = require('./connection.js');
var query = require('./query.js');
var invoke = require('./invoke.js');
var blockListener = require('./listener.js');

hfc.addConfigFile('config.json');
var host = 'localhost';
var port = 3000;
var channelName = hfc.getConfigSetting('channelName');
var chaincodeName = hfc.getConfigSetting('chaincodeName');
var peers = hfc.getConfigSetting('peers');

var username = {USERNAME};
var orgName = {ORGANIZATION_NAME};

app.options('*', cors());
app.use(cors());
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({
        extended: false
}));

app.use(function(req, res, next) {
        logger.info('New request for URL %s',req.originalUrl);
        return next();
});

const awaitHandler = (fn) => {
        return async (req, res, next) => {
                try {
                     	await fn(req, res, next)
                }
                catch (err) {
                        next(err)
                }
        }
}


function getErrorMessage(field) {
        var response = {
                success: false,
                message: field + ' field is missing or Invalid in the request'
        };
	return response;
}

var server = http.createServer(app).listen(port, function() {});
logger.info('*** SERVER STARTED ***');
logger.info('Listening on: http://%s:%s',host,port);
server.timeout = 240000;

function getErrorMessage(field) {
        var response = {
                success: false,
                message: field + ' field is missing or Invalid in the request'
        };
	return response;
}


const wss = new WebSocketServer.Server({ server });
wss.on('connection', function connection(ws) {
        ws.on('message', function incoming(message) {
                console.log('Websocket Server received message: %s', message);
        });

	ws.send('something');
});

app.get('/health', awaitHandler(async (req, res) => {
        res.sendStatus(200);
}));


app.post('/identity', awaitHandler(async (req, res) => {
        username = req.body.username;
        let response = await connection.getRegisteredUser(username, orgName, true);
    if (response && typeof response !== 'string') {
                await blockListener.startBlockListener(channelName, username, orgName, wss);
                res.json(response);
        } else {
                res.json({success: false, message: response});
        }
}));

app.post('/users', awaitHandler(async (req, res) => {
        var args = [req.body.username, req.body.balance];
        var fcn = "createUser";
        let message = await invoke.invokeChaincode(peers, channelName, chaincodeName, args, fcn, username, orgName);
        res.send(message);
}));

app.get('/supply/:username', awaitHandler(async (req, res) => {
        let args = [req.params.username];
  	    let fcn = "queryBalance";
  	    let message = await query.queryChaincode(peers, channelName, chaincodeName, args, fcn, username, orgName);
        let response = message;
        res.json(response);
}));

app.post('/transfer', awaitHandler(async (req, res) => {
        let args = [req.body.from, req.body.to, req.body.amount];
        let fcn = "transfer";
        let message = await invoke.invokeChaincode(peers, channelName, chaincodeName, args, fcn, username, orgName);
        res.send(message);
}));

app.get('/item/:uniqueId', awaitHandler(async (req, res) => {
        let args = [req.params.uniqueId];
        let fcn = "queryItem";
        let message = await query.queryChaincode(peers, channelName, chaincodeName, args, fcn, username, orgName);
    message[0].Attributes = JSON.parse(message[0].Attributes);
    var response = message;
        res.json(response);
}));

app.get('/useritems/:username', awaitHandler(async (req, res) => {
        let args = [req.params.username];
        let fcn = "queryUser";
        let message = await query.queryChaincode(peers, channelName, chaincodeName, args, fcn, username, orgName);
    var response = message;

    for (var i=0;  message[0].ownedItems != null && i < message[0].ownedItems.length ; i++){
    args = [message[0].ownedItems[i]];
    fcn = "queryItem";
        let item = await query.queryChaincode(peers, channelName, chaincodeName, args, fcn, username, orgName);
    item[0].Attributes = JSON.parse(item[0].Attributes);
    response[0].ownedItems[i] =  item[0];
    }
        res.json(response);
}));

app.post('/updateEquip', awaitHandler(async (req, res) => {
    let equippedItem =  req.body.EquippedItems;
    /* Get the list of saved items*/
    let args = [req.body.username];
    for (var i = 0; i < equippedItem.length; i++){
        args.push(equippedItem[i]);
    }
    let fcn = "updateUserEquip";
        let message = await invoke.invokeChaincode(peers, channelName, chaincodeName, args, fcn, username, orgName);
        var response = message;
        res.json(response);
}));

app.post('/updateOwn', awaitHandler(async (req, res) => {
        let ownedItem =  req.body.OwnedItems;
        let args = [req.body.username];
        for (var i = 0; i < ownedItem.length; i++){
                args.push(ownedItem[i]);
        }
    let fcn = "updateUserOwn";
        let message = await invoke.invokeChaincode(peers, channelName, chaincodeName, args, fcn, username, orgName);
        var response = message;
        res.json(response);
}));


app.post('/sellItem', awaitHandler(async (req, res) => {
        let args = [req.body.username, req.body.itemUniqueID, req.body.price];
    /*SELLING THE ITEM*/
    let fcn = "sellItem";
        let message = await invoke.invokeChaincode(peers, channelName, chaincodeName, args, fcn, username, orgName);
        var response = message;

    /*GET USER INACTIVE LIST FROM BC*/
    args = [req.body.username];
    fcn = "queryUser";
        message = await query.queryChaincode(peers, channelName, chaincodeName, args, fcn, username, orgName);
    if(message[0].inactiveItems != null){
        for (var i = 0; i < message[0].inactiveItems.length ; i++){
            args = args.push(message[0].inactiveItems[i]);}
    }
    args.push(req.body.itemUniqueID);

        fcn = "updateInactive";
        message = await invoke.invokeChaincode(peers, channelName, chaincodeName, args, fcn, username, orgName);
        response = message;

        res.json(response);
}));


app.post('/item', awaitHandler(async (req, res) => {
        let args = [req.body.UniqueID, req.body.Name, req.body.Source,req.body.Description, req.body.Slot, JSON.stringify(req.body.Attributes), req.body.ImageURL];
        let fcn = "createItem";
        let message = await invoke.invokeChaincode(peers, channelName, chaincodeName, args, fcn, username, orgName);
        res.send(message);
}));