//const { write } = require("../lib/signalr-protocol-msgpack/dist/browser/signalr-protocol-msgpack");

var endpointValue;
var accessToken = '';
var connection = {};
var btnConnect;
var btnGetMyInfo;
var txtrow;
var txtcol;
var txtidItem;
var txtdayBrr;
var txtbetValue;
var txtSessionId;
var txtnameTable;
var btnGetInventory;
var btnGetRooms;
var btnCreateRoom;
var btnBuyItem;
var btnclear;
var btnJoinRoom;
var btnLeaveRoom;
var btnSurrender;
var btnReady;
var btnDanhCo;
var EndPoints;
var btnKick;
var btnGetShop;
var output;
var userInput;
var pwInput;
var self = this;
var connection = {};

async function Start() {
    endpointValue = document.getElementById("endpoint").value;
    btnSend.disabled = true;
    btnclear.disabled = false;
    if (connection !== undefined) {
    }
    try {
        await connection.start();
        console.assert(connection.state === signalR.HubConnectionState.Connected);
        // writeToScreen("SignalR Connected. ");
    } catch (err) {
        console.log(err);
        writeToScreen('<span style="color: red;">ERROR:</span> ' + err.data);
    }
}

function loginHttp() {
    self.userInput = document.getElementById('txtUserName').value;
    self.pwInput = document.getElementById('txtPassword').value;
    var data = {
        username: userInput,
        password: pwInput
    };
    var xhttp = new XMLHttpRequest();

    xhttp.onreadystatechange = function () {
        if (this.readyState == 4 && this.status == 200) {
            self.btnConnect.disabled = false;
            self.btnSend.disabled = true;

            var jsontext = JSON.parse(this.responseText);
            self.accessToken = jsontext.data.token;
            self.endpointValue = document.getElementById("endpoint").value;

            self.userInput.disabled = true;
            self.pwInput.disabled = true;

            console.log(self.accessToken);
            console.log(self.endpointValue);


            connection = new signalR.HubConnectionBuilder()
                .withUrl(self.endpointValue, {
                    accessTokenFactory: () => self.accessToken

                })
                .build();


            connection.on('ConnectSucess', () => {
                writeToScreen("Connect Sucess !!!");
                writeToScreen("Token :" + self.accessToken);
                btnCreateRoom.disabled = false;
                btnJoinRoom.disabled = false;
                btnGetInventory.disabled = false;
                btnGetMyInfo.disabled = false;
                btnGetRooms.disabled = false;

                btnSend.disabled = true;
                btnGetShop.disabled = false;
                btnConnect.disabled = true;
                endpoint.disabled = true;
            })

            connection.on('ConnectFailed', () => {
                writeToScreen("Connect Failed , Try Again !!!");
                connection.stop();
                btnConnect.disabled = true;
                btnSend.disabled = false;
            })

            connection.on('PlayerCaro', (player) => {
                console.log(player);
                self.writeToScreen('<span style="color: blue;">RESPONSE:Account_Info: ' + `${JSON.stringify(player)}` + '</span>');
            });

            connection.on('Inventory', (listInventory) => {
                console.log(listInventory);
                self.writeToScreen('<span style="color: blue;">RESPONSE:Inventory: ' + `${JSON.stringify(listInventory)}` + '</span>');
            });

            connection.on('GetSession', (listrooms) => {
                console.log(listrooms);
                self.writeToScreen('<span style="color: blue;">RESPONSE:Rooms: ' + `${listrooms}` + '</span>');
            });

            connection.on('SessionCreated', (sessionId, nameTable) => {
                console.log(sessionId);
                self.writeToScreen('Create Room sessionID = : ' + `${sessionId}`);
                self.writeToScreen('nameTable = :' + `${nameTable}`);
                btnCreateRoom.disabled = true;
                btnJoinRoom.disabled = true;
                btnLeaveRoom.disabled = false;
            });

            connection.on('NameInvalid', () => {
                self.writeToScreen('NameRoom Exists !!!');
                btnLeaveRoom.disabled = true;
            })

            connection.on('SessionJoined', (connection1, connection2) => {
                self.writeToScreen('PlayerConnection 1 = : ' + `${connection1}`);
                self.writeToScreen('PlayerConnection 2 = : ' + `${connection2}`);
                btnCreateRoom.disabled = true;
                btnJoinRoom.disabled = true;
                btnReady.disabled = false;
                btnLeaveRoom.disabled = false;
            });

            connection.on('SessionNotFound', () => {
                self.writeToScreen('Room does not exist !!!');
                btnLeaveRoom.disabled = true;
            });

            connection.on('RoomIsFull', () => {
                self.writeToScreen('Room is full !!!');
                btnLeaveRoom.disabled = true;
            });

            connection.on('SomeOneJoin', () => {
                btnKick.disabled = false;
            });

            connection.on('AllreadyToPlay', () => {
                self.writeToScreen('Both Player Ready, Lets Start !!!');
                btnReady.disabled = true;
                btnLeaveRoom.disabled = true;
                btnDanhCo.disabled = true;
                btnSurrender.disabled = false;
                btnKick.disabled = true;
            });

            connection.on('ReadyToPlay', (connectionId) => {
                self.writeToScreen('Player ' + `${connectionId}` + '  ready !!!');

            });

            connection.on('PartnerLeaveRoom', (connectionIdleave) => {
                self.writeToScreen('PlayerConnection  = : ' + `${connectionIdleave}` + '  exit room !!!');
                btnReady.disabled = true;
                btnSurrender.disabled = true;
                btnKick.disabled = true;
            })

            connection.on('DisconnectedWhileInRoom', (connectionExit) => {
                console.log(connectionExit + 'exit room ');
                self.writeToScreen('PlayerConnection  = : ' + `${connectionExit}` + '  exit room !!!');
                btnReady.disabled = true;
                btnLeaveRoom.disabled = false;
                btnSurrender.disabled = true;
                btnKick.disabled = true;
            });

            connection.on('DisconnectedWhileInGame', (connectionExit) => {
                console.log(connectionExit + 'exit room ');
                self.writeToScreen('Your opposite had disconnected');
                self.writeToScreen('You are winner !!!');
                btnReady.disabled = true;
                btnLeaveRoom.disabled = false;
                btnDanhCo.disabled = true;
                btnSurrender.disabled = true;
                btnKick.disabled = true;
            });

            connection.on('PlayFirst', () => {
                self.writeToScreen('You go first');
                btnDanhCo.disabled = false;
            })
            connection.on('msgingame', (msg) => {
                self.writeToScreen('Message' + `${msg}`);
                btnDanhCo.disable = false;
            })

            connection.on('Winner', () => {
                self.writeToScreen('You are winner !!!');
                btnReady.disabled = false;
                btnSurrender.disabled = true;
                btnLeaveRoom.disabled = false;
                btnDanhCo.disabled = true;
            });

            connection.on('Loser', () => {
                self.writeToScreen('You Lose !!!');
                btnReady.disabled = false;
                btnSurrender.disabled = true;
                btnLeaveRoom.disabled = false;
                btnDanhCo.disabled = true;
            });


            connection.on('GameDraw', () => {
                self.writeToScreen('Drawwww !!!');
                btnReady.disabled = false;
                btnSurrender.disabled = true;
                btnLeaveRoom.disabled = false;
                btnDanhCo.disabled = true;
            });
            connection.on('RoomMaster', () => {
                btnKick.disabled = false;
            })

            connection.on('KickPartner', () => {
                btnKick.disabled = true;
                btnReady.disabled = true;
                btnDanhCo.disabled = true;
                btnLeaveRoom.disabled = false;
                self.writeToScreen('You kick your opposite!!!');
            })

            connection.on('Kicked', () => {
                btnJoinRoom.disabled = false;
                btnCreateRoom.disabled = false;
                btnLeaveRoom.disabled = true;
                btnReady.disabled = true;
                self.writeToScreen('You has been kicked from room !!!');
            })

            connection.on('YouMove', (row, col, value) => {
                self.writeToScreen('You check  ' + ` ${value}` + '  row ' + `${row}` + ' col ' + `${col}`);

            });
            connection.on('EnemyMove', (row, col, value) => {
                self.writeToScreen('Opposite check  ' + ` ${value}` + '  row ' + `${row}` + ' col ' + `${col}`);
                btnDanhCo.disabled = false;
            });

            connection.on('GetShop', (listitems) => {
                writeToScreen('Shop List Items : ' + `${JSON.stringify(listitems)}`);
            });

            connection.on('BuySuccess', (iditem) => {
                writeToScreen("Buy successful Item id : " + `${iditem}`);
            });

            connection.on('BuyFailed', (iditem) => {
                writeToScreen("buy itemId: " + `${iditem}` +" failed");
            });
        }

    };
    xhttp.open("POST", "http://localhost:4600/api/auth/login", true);

    xhttp.setRequestHeader("Content-type", "application/json");
    xhttp.setRequestHeader("Access-Control-Allow-Origin", "*");

    var requestData = JSON.stringify(data);
    xhttp.send(requestData);
}
// self executing function here


function clearOutput() {
    output.innerHTML = "";
}



function getMyInfo() {
    connection.invoke("GetAccountInfo",);
    writeToScreen("SENT_GetMyInfo: ");
}

function getRooms() {
    connection.invoke("GetRooms",);
    writeToScreen("SENT_GetRooms: ");
}

function getInventory() {
    connection.invoke("GetInventory",);
    writeToScreen("SENT_GetInventory: ");
}

function writeToScreen(message) {
    var pre = document.createElement("p");
    pre.style.wordWrap = "break-word";
    pre.innerHTML = message;
    output.appendChild(pre);
}
function joinRoom() {
    txtSessionId = document.getElementById("txtSessionId").value;
    if (txtSessionId !== null && txtSessionId !== '') {
        connection.invoke("JoinRoomWithName", txtSessionId);
        writeToScreen("Entering Room ... ");
        
    }
    else {
        writeToScreen("SessionId is null ...");
    }
}

function getShop() {
    connection.invoke("GetShop");
}

function CreateRoom() {
    txtbetValue = parseInt(document.getElementById("txtbetValue").value);
    txtnameTable = document.getElementById("txtnameTable").value;

    if (txtbetValue !== '' && txtnameTable !== '') {
        connection.invoke('CreateSession', txtbetValue, txtnameTable);
    }
    else {
        writeToScreen("Lack of information ...");
    }
}

function readyStart() {
    connection.invoke('ReadyPlay');
    btnReady.disabled = true;
    btnKick.disabled = true;
}
function leaveRoom() {
    connection.invoke('LeaveRoom');
    writeToScreen('you had leave the room !!!');
    btnCreateRoom.disabled = false;
    btnJoinRoom.disabled = false;
    btnReady.disabled = true;
    btnDanhCo.disabled = true;
    btnLeaveRoom.disabled = true;
}
function surrender() {
    connection.invoke('Surrender');
    writeToScreen('you are loser !!!');
    btnLeaveRoom.disabled = false;
    btnDanhCo.disabled = true;
    btnReady.disabled = false;
    btnSurrender.disabled = true;
}

function danhCo() {
    txtrow = parseInt(document.getElementById('txtToaDoX').value);
    txtcol = parseInt(document.getElementById('txtToaDoY').value);
    if (txtrow !== '' && txtcol !== '') {
        connection.invoke('MakeMove', txtrow, txtcol);
        btnDanhCo.disabled = true;
    }
    else {
        writeToScreen("Lack of information ...");
    }

}

function BuyItem() {
    txtidItem = parseInt(document.getElementById('txtidItem').value);
    txtdayBrr = parseInt(document.getElementById('txtdayBrr').value);
    if (txtidItem !== '' && txtdayBrr !== '') {
        connection.invoke("BuyItem", txtidItem, txtdayBrr);
    }
    
}

function Kick() {
    connection.invoke('KickPlayer');
    btnKick.disabled = true;
    btnReady.disabled = true;
}

document.addEventListener('DOMContentLoaded', function () {
    output = document.getElementById("rightOutput");
    btnSend = document.getElementById("btnSend");


    btnConnect = document.getElementById("btnConnect");
    btnConnect.disabled = true;

    btnGetMyInfo = document.getElementById("btnGetMyInfo");
    btnGetMyInfo.disabled = true;

    btnGetInventory = document.getElementById("btnGetInventory");
    btnGetInventory.disabled = true;

    btnGetRooms = document.getElementById("btnGetRooms");
    btnGetRooms.disabled = true;

    btnCreateRoom = document.getElementById("btnCreateRoom");
    btnCreateRoom.disabled = true;

    btnJoinRoom = document.getElementById("btnJoinRoom");
    btnJoinRoom.disabled = true;

    btnLeaveRoom = document.getElementById("btnLeaveRoom");
    btnLeaveRoom.disabled = true;

    btnSurrender = document.getElementById("btnSurr");
    btnSurrender.disabled = true;

    btnReady = document.getElementById("btnReadyStart");
    btnReady.disabled = true;

    btnDanhCo = document.getElementById("btnDanhCo");
    btnDanhCo.disabled = true;

    btnKick = document.getElementById("btnKick");
    btnKick.disabled = true;

    btnclear = document.getElementById("btnclear");
    btnclear.disabled = true;

    btnGetShop = document.getElementById("btnGetShop");
    btnGetShop.disabled = true;

    btnBuyItem = document.getElementById("btnBuyItem");
    btnBuyItem.disabled = false;
}, false);