function showPlaylists(bShowPlaylists) {

    const spotifyPlaylistsDisplay = document.getElementById("spotifyPlaylistsDisplay");
    const spotifyPlaylistDisplay = document.getElementById("spotifyPlaylistDisplay");
    const aiPrompts = document.getElementById("aiPrompts");
    const btnCreatePlaylist = document.getElementById("btn_CreatePlaylist");

    spotifyPlaylistDisplay.style.display = "none";

    if (!bShowPlaylists) {
        spotifyPlaylistsDisplay.style.display = "none";
        aiPrompts.style.display = "";
    }
    else {
        spotifyPlaylistsDisplay.style.display = "";
        aiPrompts.style.display = "none";
        btnCreatePlaylist.style.display = "none";
    }
}

function spotifySetSearchDisplay(jsonDataUser, jsonListPlaylists) {

    spotifyDisplayUserInfo(jsonDataUser);
    spotifyDisplayPlaylists(jsonListPlaylists);
}
function spotifyDisplayUserInfo(jsonDataUser) {
    //console.debug("spotifyDisplayUserInfo");

    const obj = JSON.parse(jsonDataUser);
    document.getElementById("user_id").innerText = obj.SpotifyID;
}
function displayNavigation() {
    document.getElementById("div_Sidebar").style.display = "";
}
function spotifyDisplayPlaylists(jsonListPlaylists) {
    
    const obj = JSON.parse(jsonListPlaylists);

    let elmImg, elmDiv, elmHeading, elmB;

    const spotifyPlaylistsDisplay = document.getElementById("spotifyPlaylistsDisplay");
    const arrClasses = ["w3-card", "w3-bar-item", "w3-round-xlarge"];

    for (index in obj) {

        elmDiv = document.createElement("div");

        for (let i = 0; i < arrClasses.length; i++) {
            elmDiv.classList.add(arrClasses[i]);
        }

        elmDiv.style.paddingTop = "16px";
        elmDiv.style.margin = "8px";
        elmDiv.style.cursor = "pointer";
        elmDiv.setAttribute("onclick", `spotifyOpenPlaylist('${obj[index].Id}', '${obj[index].Name}')`);

        elmImg = document.createElement("img");
        elmImg.classList.add("w3-round-large");
        if (obj[index].Images != null) {
            elmImg.src = obj[index].Images[0].Url;
        }
        elmImg.style.width = "200px"
        elmImg.style.height = "200px"
        elmDiv.appendChild(elmImg);

        elmHeading = document.createElement("h5");
        elmHeading.style.textAlign = "right";

        elmB = document.createElement("b");
        elmB.innerText = spotifyAlterString(obj[index].Name);
        elmHeading.appendChild(elmB);
        elmDiv.appendChild(elmHeading);

        spotifyPlaylistsDisplay.appendChild(elmDiv);
    }
}
function spotifyOpenPlaylist(playlistName) {
    
    document.getElementById("spotifyPlaylistsDisplay").style.display = "none";
    document.getElementById("spotifyPlaylistDisplay").style.display = "";
    document.getElementById("playlistName").innerText = playlistName;

}
//function testAsyncTask() {
//    console.debug("testAsyncTask START!");
//    DotNet.invokeMethodAsync('BlazorApp1', 'TestTask');
//}
function spotifyAlterString(str) {

    const strLength = str.length;
    const strSpacer = "...";

    if (strLength > 20) {
        str = str.substring(0, 17) + strSpacer;
    }

    return str;
}
function spotifyProcessChatResponse(jsonData) {

    console.debug(`spotifyProcessChatResponse: START`);
    const aiGeneratedPlaylist = document.getElementById("ai_GeneratedPlaylist");
    const aiListPlaylistTracks = document.getElementById("ai_ListPlaylistTracks");
    document.getElementById("btn_CreatePlaylist").style.display = "";

    //console.debug(`spotifyProcessChatResponse jsonData: ${jsonData}`);
    let obj = JSON.parse(jsonData);
    //console.debug(`spotifyProcessChatResponse description: ${obj}`);
    //console.debug(`spotifyProcessChatResponse description: ${obj.stringify()}`);
    document.getElementById("input_Prompt").value = "";

    document.getElementById("ai_PlaylistDescription").innerText = obj.Description;

    for (var index in obj.Playlist) {
        console.debug(`spotifyProcessChatResponse adding: ${obj.Playlist[index].Title} - ${obj.Playlist[index].Artist}.`);
        var elmLi = document.createElement("li");

        elmLi.setAttribute("spotify_id", obj.Playlist[index].Spotify_Id);
        elmLi.setAttribute("track_name", obj.Playlist[index].Title);
        elmLi.setAttribute("track_artist", obj.Playlist[index].Artist);
        elmLi.style.listStyleType = "none";

        elmLi.innerText = `${obj.Playlist[index].Title} - ${obj.Playlist[index].Artist}`;

        aiListPlaylistTracks.appendChild(elmLi);
    }

    const arrIDsToHide = ["display_Loading", "spotifyPlaylistDisplay", "spotifyPlaylistsDisplay"];

    for (let i = 0; i < arrIDsToHide.length; i++) {
        document.getElementById(arrIDsToHide[i]).style.display = "none";
    }
    aiGeneratedPlaylist.style.display = "";
}

function showLoading() {
    document.getElementById("display_Loading").style.display = "";
    document.getElementById("spotifyPlaylistDisplay").style.display = "none";
    document.getElementById("spotifyPlaylistsDisplay").style.display = "none";
}

function getTracks() {

    let listTracks = [];

    const aiListPlaylistTracks = document.getElementById("ai_ListPlaylistTracks");
    const childLi = aiListPlaylistTracks.getElementsByTagName("li");

    for (let i = 0; i < childLi.length; i++) {
        let spotifyId = childLi[i].getAttribute("spotify_id");
        //let spotifyTrackName = childLi[i].getAttribute("track_name");
        //let spotifyTrackArtist = childLi[i].getAttribute("track_artist");

        //let CreateTrack = {
          //  artist: spotifyTrackArtist,
            //name: spotifyTrackName
        //}
        listTracks.push(spotifyId);
    }

    return JSON.stringify(listTracks);
}
function spotTest(displayName) {
    document.getElementById("user_id").innerText = displayName;
}
