function spotifySetSearchDisplay(jsonDataUser, jsonListPlaylists) {

    spotifyDisplayUserInfo(jsonDataUser);
    spotifyDisplayPlaylists(jsonListPlaylists);

}
function spotifyDisplayUserInfo(jsonDataUser) {
    console.debug("spotifyDisplayUserInfo");

    let obj = JSON.parse(jsonDataUser);

    document.getElementById("user_id").innerText = obj.SpotifyID;
}
function spotifyDisplayPlaylists(jsonListPlaylists) {
    
    let obj = JSON.parse(jsonListPlaylists);

    let elm_img, elm_div, elm_h, elm_b;

    const spotifyPlaylistsDisplay = document.getElementById("spotifyPlaylistsDisplay");

    const arrClasses = ["w3-card", "w3-bar-item", "w3-round-xlarge"];

    for (index in obj) {

        elm_div = document.createElement("div");

        for (let i = 0; i < arrClasses.length; i++) {
            elm_div.classList.add(arrClasses[i]);
        }

        elm_div.style.paddingTop = "16px";
        elm_div.style.margin = "8px";
        elm_div.style.cursor = "pointer";
        elm_div.setAttribute("onclick", `spotifyOpenPlaylist('${obj[index].Id}', '${obj[index].Name}')`);

        elm_img = document.createElement("img");
        elm_img.classList.add("w3-round-large");
        if (obj[index].Images != null) {
            elm_img.src = obj[index].Images[0].Url;
        }
        elm_img.style.width = "200px"
        elm_img.style.height = "200px"
        elm_div.appendChild(elm_img);

        elm_h = document.createElement("h5");
        elm_h.style.textAlign = "right";

        elm_b = document.createElement("b");
        elm_b.innerText = spotifyAlterString(obj[index].Name);
        elm_h.appendChild(elm_b);
        elm_div.appendChild(elm_h);

        spotifyPlaylistsDisplay.appendChild(elm_div);
    }
}
function spotifyOpenPlaylist(playlistName) {
    //var selectedPlaylist = document.getElementById("selectedPlaylist");
    //selectedPlaylist.innerHTML = "";

    document.getElementById("spotifyPlaylistsDisplay").style.display = "none";
    document.getElementById("spotifyPlaylistDisplay").style.display = "";

    document.getElementById("playlistName").innerText = playlistName;

}
function testAsyncTask() {
    console.debug("testAsyncTask START!");
    DotNet.invokeMethodAsync('BlazorApp1', 'TestTask');
}
function spotifyAlterString(str) {

    const strLength = str.length;
    const strSpacer = "...";

    if (strLength > 20) {
        str = str.substring(0, 17) + strSpacer;
    }

    return str;
}
function spotifyProcessChatResponse(jsonData) {

    var aiGeneratedPlaylist = document.getElementById("ai_GeneratedPlaylist");
    var aiListPlaylistTracks = document.getElementById("ai_ListPlaylistTracks");

    let obj = JSON.parse(jsonData);

    document.getElementById("input_Prompt").value = "";
    document.getElementById("ai_PlaylistDescription").innerText = obj.description;

    for (var index in obj.playlist) {

        var elm_Li = document.createElement("li");

        elm_Li.setAttribute("spotify_id", obj.playlist[index].spotify_id);
        elm_Li.style.listStyleType = "none";

        elm_Li.innerText = `${obj.playlist[index].title} - ${obj.playlist[index].artist}`;

        aiListPlaylistTracks.appendChild(elm_Li);
    }

    document.getElementById("display_Loading").style.display = "none";
    aiGeneratedPlaylist.style.display = "";
    document.getElementById("spotifyPlaylistDisplay").style.display = "none";
    document.getElementById("spotifyPlaylistsDisplay").style.display = "none";
}

function showLoading() {
    document.getElementById("display_Loading").style.display = "";
    document.getElementById("spotifyPlaylistDisplay").style.display = "none";
    document.getElementById("spotifyPlaylistsDisplay").style.display = "none";
}

function getTracks() {

    var listTrackIds = [];

    var aiListPlaylistTracks = document.getElementById("ai_ListPlaylistTracks");
    var child_Li = aiListPlaylistTracks.getElementsByTagName("li");

    for (var i = 0; i < child_Li.length; i++) {
        var spotifyTrackId = child_Li[i].getAttribute("spotify_id");
        listTrackIds.push(spotifyTrackId);
    }

    return listTrackIds;
}

function tryMe() {
    return "I'M ALIVE!!!";
}