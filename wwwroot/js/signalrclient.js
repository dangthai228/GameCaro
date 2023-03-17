var endpointValue;
var accessToken = '';
var connection = {};
var btnConnect;
var btnGetMyInfo;
var txtrow;
var txtcol;
var txtbetValue;
var txtSessionId;
var txtnameTable;
var btnGetInventory;
var btnGetRooms;
var btnCreateRoom;
var btnJoinRoom;
var btnLeaveRoom;
var btnSurrender;
var btnReady;
var btnDanhCo;
var EndPoints;
var output;
var userInput;
var pwInput;
var self = this;
var connection = {};

async function Start() {
    endpointValue = document.getElementById("endpoint").value;
    btnSend.disabled = true;
    if (connection !== undefined) {
    }
    try {
        await connection.start();
        console.assert(connection.state === signalR.HubConnectionState.Connected);

        btnCreateRoom.disabled = false;
        btnJoinRoom.disabled = false;
        btnGetInventory.disabled = false;
        btnGetMyInfo.disabled = false;
        btnGetRooms.disabled = false;

        btnConnect.disabled = true;
        endpoint.disabled = true;
        console.log("SignalR Connected.");
        writeToScreen("SignalR Connected. ");
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
            writeToScreen("Login Sucess !!!");
            writeToScreen("Token :" + self.accessToken);
           

             connection = new signalR.HubConnectionBuilder()
                .withUrl(self.endpointValue, {
                    accessTokenFactory: () => self.accessToken

                })
                .build();
            

           

            connection.on('PlayerCaro', (player) => {
                console.log(player);
                self.writeToScreen('<span style="color: blue;">RESPONSE:Account_Info: ' + `${JSON.stringify(player) }` + '</span>');
            });

            connection.on('Inventory', (listInventory) => {
                console.log(listInventory);
                self.writeToScreen('<span style="color: blue;">RESPONSE:Inventory: ' + `${JSON.stringify(listInventory)}` + '</span>');
            });

            connection.on('GetSession', (listrooms) => {
                console.log(listrooms);
                self.writeToScreen('<span style="color: blue;">RESPONSE:Rooms: ' + `${listrooms}` + '</span>');
            });

            connection.on('SessionCreated', (sessionId , nameTable) => {    
                console.log(sessionId);
                self.writeToScreen('Create Room sessionID = : ' + `${sessionId}` );
                self.writeToScreen('nameTable = :' + `${nameTable}`);
                btnCreateRoom.disabled = true;
                btnJoinRoom.disabled = true;
            });


            connection.on('SessionJoined', (connection1, connection2) => {
                self.writeToScreen('PlayerConnection 1 = : ' + `${connection1}`);
                self.writeToScreen('PlayerConnection 2 = : ' + `${connection2}`);
                btnCreateRoom.disabled = true;
                btnJoinRoom.disabled = true;
                btnReady.disabled = false;
            });

            connection.on('AllreadyToPlay', () => {
                self.writeToScreen('Both Player Ready, Lets Start !!!');
                btnReady.disabled = true;
                btnLeaveRoom.disabled = true;
                btnDanhCo.disabled = true;
                btnSurrender.disabled = false;
            });

            connection.on('ReadyToPlay', (connectionId) => {
                self.writeToScreen('Player ' + `${connectionId}` + '  ready !!!');
                
            });

            connection.on('PartnerLeaveRoom', (connectionIdleave) => {
                self.writeToScreen('PlayerConnection  = : ' + `${connectionIdleave}` + '  exit room !!!');
                btnReady.disabled = true;
                btnSurrender.disabled = true;
            })

            connection.on('DisconnectedWhileInRoom', (connectionExit) => {
                console.log(connectionExit + 'exit room ');
                self.writeToScreen('PlayerConnection  = : ' + `${connectionExit}` + '  exit room !!!');
                btnReady.disabled = true;
                btnLeaveRoom.disabled = false;
                btnSurrender.disabled = true;
            });

            connection.on('DisconnectedWhileInGame', (connectionExit) => {
                console.log(connectionExit + 'exit room ');
                self.writeToScreen('Your opposite had disconnected');
                self.writeToScreen('You are winner !!!');
                btnReady.disabled = true;
                btnLeaveRoom.disabled = false;
                btnDanhCo.disabled = true;
                btnSurrender.disabled = true;
            });

            connection.on('PlayFirst', () => {
                self.writeToScreen('You go first');
                btnDanhCo.disabled = false;
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

            connection.on('YouMove', (row, col, value) => {
                self.writeToScreen('You check  ' + ` ${value}` + '  row ' + `${row}` + ' col ' + `${col}`);
               
            });
            connection.on('EnemyMove', (row, col, value) => {
                self.writeToScreen('Opposite check  ' + ` ${value}` + '  row ' + `${row}` + ' col ' + `${col}`);
                btnDanhCo.disabled = false;
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
    connection.invoke("GetAccountInfo", );
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
    connection.invoke("JoinSession", txtSessionId);
    writeToScreen("Entering Room ... ");
    btnLeaveRoom.disabled = false;
}

function CreateRoom() {
    txtbetValue = parseInt(document.getElementById("txtbetValue").value);
    txtnameTable = document.getElementById("txtnameTable").value;
    connection.invoke('CreateSession', txtbetValue, txtnameTable);
    btnLeaveRoom.disabled = false;
}

function readyStart() {
    connection.invoke('ReadyPlay');
    btnReady.disabled = true;
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
    connection.invoke('MakeMove', txtrow, txtcol);
    btnDanhCo.disabled = true;
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
    
}, false);