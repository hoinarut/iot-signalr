"use strict";

const connection = new signalR.HubConnectionBuilder().withUrl("/devices?isDevice=false").build();

connection.on("DeviceConnected", function (device) {
    removeNoDevices();
    addDeviceToTable(device);
});
connection.on("DeviceDisconnected", function (device) {
    console.log(`Client ${device} disconnected`);
    removeDevice(device);
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
    table.style.border = '1px solid black';
    const tableHeader = document.createElement("th");
    tableHeader.insertCell("Device Id");
    tableHeader.insertCell("Last Poll Time");
    table.appendChild(tableHeader)
    document.getElementById("devices").appendChild(table);
    return table;
}

function addDeviceToTable(device) {
    const devicesElem = getDevicesElement();
    const row = devicesElem.insertRow(-1);
    row.setAttribute("id", device.deviceId);
    const deviceIdCell = row.insertCell(0);
    const lastPollTimeCell = row.insertCell(1);

    deviceIdCell.innerHTML = device.deviceId;
    lastPollTimeCell.innerHTML = device.lastPollTime;
}

function removeDevice(device) {
    const deviceElement = document.getElementById(device.deviceId);
    if (deviceElement) {
        deviceElement.remove();
        const tableElem = getDevicesElement();
        if (!tableElem.children.length) {
            tableElem.remove();
            addNoDevices();
        }
    }
}