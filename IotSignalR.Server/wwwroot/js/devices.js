"use strict";

const connection = new signalR.HubConnectionBuilder().withUrl("/devices?isDevice=false").build();

connection.on("DeviceConnected", function (device) {
    removeNoDevices();
    addDeviceToTable(device);
});
connection.on("DeviceDisconnected", function (device) {
    console.log(`Client ${device.deviceId} disconnected`);
    updateDevice(device);
});

connection.on("OnHeartbeat", function (event) {
    console.log(`Heartbeat from ${event.deviceId} at ${event.timeStamp}`);
    updateDeviceLastPollTime(event.deviceId, event.timeStamp);
});

connection.start().then(async function () {
    await connection.send("RegisterManager");
    try {
        const devices = await connection.invoke("GetAllDevices");
        if (devices?.length) {
            removeNoDevices();
            getDevicesElement();
            devices.map(device => addDeviceToTable(device));
        } else {
            addNoDevices();
        }
    } catch (err) {
        console.error(`Error retrieving device list: ${err}`);
    }
}).catch(function (err) {
    return console.error(err.toString());
});

function addNoDevices() {
    const noDevices = document.createElement("h5");
    noDevices.innerText = "No connected devices";
    noDevices.setAttribute("class", "text-center");
    noDevices.setAttribute("id", "nodevices");
    document.getElementById("devices").appendChild(noDevices);
}

function removeNoDevices() {
    const noDevices = document.getElementById("nodevices");
    if (noDevices) {
        noDevices.remove();
    }
}

function getDevicesElement() {
    const tableElem = document.getElementById("deviceList");
    if (tableElem) {
        return tableElem;
    } else {
        return createDevicesElement();
    }
}

function createDevicesElement() {
    const table = document.createElement("table");
    table.setAttribute("id", "deviceList");
    table.classList.add("table");
    const tableHeader = document.createElement("tr");
    const deviceIdCell = document.createElement("th");
    deviceIdCell.innerHTML = "Device ID";
    tableHeader.appendChild(deviceIdCell);
    const lastPollTimeCell = document.createElement("th");
    lastPollTimeCell.innerHTML = "Last Poll Time";
    tableHeader.appendChild(lastPollTimeCell);
    const connectionStateCell = document.createElement("th");
    connectionStateCell.innerHTML = "Connection State";
    tableHeader.appendChild(connectionStateCell);
    table.appendChild(tableHeader)
    document.getElementById("devices").appendChild(table);
    return table;
}

function addDeviceToTable(device) {
    const deviceRow = document.getElementById(device.deviceId);
    if (deviceRow) {
        updateDevice(device);
    } else {
        const devicesElem = getDevicesElement();
        const row = devicesElem.insertRow(-1);
        row.classList.add("device");
        row.setAttribute("id", device.deviceId);
        const deviceIdCell = row.insertCell(0);
        const lastPollTimeCell = row.insertCell(1);
        const connectionStateCell = row.insertCell(2);

        deviceIdCell.innerHTML = device.deviceId;
        lastPollTimeCell.innerHTML = formatDate(device.lastPollTime);
        connectionStateCell.innerHTML = device.isConnected ? "Online" : "Offline";
        connectionStateCell.style.backgroundColor = device.isConnected ? "green" : "indianred";
        connectionStateCell.style.color = "white";
        connectionStateCell.style.fontWeight = "bold";
    }
}

function updateDevice(device) {
    const deviceElement = document.getElementById(device.deviceId);
    if (deviceElement) {
        const lastPollTimeCell = deviceElement.children.item(1);
        lastPollTimeCell.innerHTML = formatDate(device.lastPollTime);
        const connectionStateCell = deviceElement.children.item(2);
        connectionStateCell.innerHTML = device.isConnected ? "Online" : "Offline";
        connectionStateCell.style.backgroundColor = device.isConnected ? "green" : "indianred";
    }
}

function removeDevice(deviceId) {
    const deviceElement = document.getElementById(deviceId);
    if (deviceElement) {
        deviceElement.remove();
        const tableElem = getDevicesElement();
        const devices = tableElem.getElementsByClassName("device");
        if (!devices.length) {
            tableElem.remove();
            addNoDevices();
        }
    }
}

function updateDeviceLastPollTime(deviceId, timeStamp) {
    const deviceElement = document.getElementById(deviceId);
    if (deviceElement) {
        const lastPollTimeCell = deviceElement.cells[1];
        lastPollTimeCell.innerHTML = formatDate(timeStamp);
        lastPollTimeCell.classList.add("updated");
        const animated = document.querySelector(".updated");
        animated.addEventListener("animationend", () => {
            lastPollTimeCell.classList.remove("updated");
        });
    }
}

function formatDate(date) {
    return new Date(date).toLocaleString().replace(",", "");
}