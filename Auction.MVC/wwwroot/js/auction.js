"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("https://localhost:7009/auction/hub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var li = document.createElement("li");
    document.getElementById("auction-info").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you
    // should be aware of possible script injection concerns.
    console.log("Inside ReceiveMessage" + user + " " + message);
    li.textContent = `${user}: ${message}`;
});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
    console.log("starting connection")
    var fio = document.getElementById("userFio").value;
    var iin = document.getElementById("userIin").value;
    var tradeId = document.getElementById("tradeId").value;
    var connectionParam = { TradeId: Number(tradeId), Fio: fio, Iin: iin };
    connection.invoke("JoinAuction", connectionParam).catch(function (err) {
        return console.error(err.toString());
    });
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userFio").value;
    var iin = document.getElementById("userIin").value;
    var amount = document.getElementById("newAmount").value;
    var message = user + " increases price to " + amount;
    console.log("sending message");
    connection.invoke("SetIncreaseInfo", iin, amount).catch(function (err) {
        return console.error(err.toString());
    });
    
});

document.getElementById("leaveButton").addEventListener("click", function (event) {
    console.log("leaving auction");
    connection.invoke("OnDisconnected", null).catch(function (err) {
        return console.error(err.toString());
    });

});