"use strict";

const connection = new signalR.HubConnectionBuilder().withUrl("/devices?isDevice=false").build();

connection.on("DeviceConnected", function (connectionId) {
    const noDevices = document.getElementById("nodevices");
    if (noDevices) {
        noDevices.remove();
    }
    const listElem = getDevicesListElement();
    const li = document.createElement("li");
    li.setAttribute("id", connectionId);
    li.textContent = connectionId;
    listElem.appendChild(li);
});
connection.on("DeviceDisconnected", function (connectionId) {
    console.log(`Client ${connectionId} disconnected`);
    const deviceElement = document.getElementById(connectionId);
    if (deviceElement) {
        deviceElement.remove();
        const listElem = getDevicesListElement();
        if (!listElem.children.length) {
            addNoDevices();
        }
    }
});

connection.start().then(async function () {
    await connection.send("RegisterManager");
    try {
        const devices = await connection.invoke("GetAllDevices");
        if (devices?.length) {
            const listElem = getDevicesListElement();
            listElem.setAttribute("id", "deviceList");
            let listContent = "";
            $.each(devices, (device) => {
                listContent += `<li id="${device}">${device}</li>`;
            })
            listElem.innerHTML = listContent;
            document.getElementById("devices").appendChild(listElem);
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

function getDevicesListElement() {
    const listElem = document.getElementById("deviceList");
    if (listElem) {
        return listElem;
    } else {
        const list = document.createElement("ul",)
        list.setAttribute("id", "deviceList");
        document.getElementById("devices").appendChild(list);
        return list;
    }
}