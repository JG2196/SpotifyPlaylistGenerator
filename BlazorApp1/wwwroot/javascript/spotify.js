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
//function populatePlaylists(authUserData) {

//    const obj = JSON.parse(authUserData);
//    const spotifyPlaylistsDisplay = document.getElementById("spotifyPlaylistsDisplay");

//    document.getElementById("user_id").innerText = obj.SpotifyAuthUser.DisplayName;

//    if (obj.ListSpotifyPlaylists.Playlists.length == 0) {
//        return;
//    }

//    for (let index in obj.ListSpotifyPlaylists.Playlists) {

//        let elm_Div = document.createElement("div");
//        let elm_Img = document.createElement("img");
//        let elm_H5 = document.createElement("h5");
//        let elm_B = document.createElement("b");

//        elm_Div.classList.add("w3-card");
//        elm_Div.classList.add("w3-bar-item");
//        elm_Div.classList.add("w3-round-xlarge");
//        elm_Div.setAttribute("onclick", `@OpenPlaylist(${obj.ListSpotifyPlaylists.Playlists[index].Id}, ${obj.ListSpotifyPlaylists.Playlists[index].Name})`);
//        elm_Div.style.paddingTop = "16px";
//        elm_Div.style.margin = "8px";
//        elm_Div.style.cursor = "pointer";

//        elm_Img.classList.add("w3-round-large");
//        if (obj.ListSpotifyPlaylists.Playlists[index].Images != null) {
//            elm_Img.src = obj.ListSpotifyPlaylists.Playlists[index].Images[0].Url;
//        }
//        elm_Img.style.width = "200px";
//        elm_Img.style.height = "200px";

//        elm_H5.style.textAlign = "right";
//        elm_B.innerText = obj.ListSpotifyPlaylists.Playlists[index].Name;

//        elm_H5.appendChild(elm_B);
//        elm_Div.appendChild(elm_Img);
//        elm_Div.appendChild(elm_H5);
//        spotifyPlaylistsDisplay.appendChild(elm_Div);
//    }
//}
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

    const aiGeneratedPlaylist = document.getElementById("ai_GeneratedPlaylist");
    const aiListPlaylistTracks = document.getElementById("ai_ListPlaylistTracks");
    document.getElementById("btn_CreatePlaylist").style.display = "";

    let obj = JSON.parse(jsonData);

    document.getElementById("input_Prompt").value = "";
    document.getElementById("ai_PlaylistDescription").innerText = obj.description;

    for (var index in obj.playlist) {

        var elmLi = document.createElement("li");

        //elm_Li.setAttribute("spotify_id", obj.playlist[index].spotify_id);
        elmLi.setAttribute("track_name", obj.playlist[index].title);
        elmLi.setAttribute("track_artist", obj.playlist[index].artist);
        elmLi.style.listStyleType = "none";

        elmLi.innerText = `${obj.playlist[index].title} - ${obj.playlist[index].artist}`;

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
        let spotifyTrackName = childLi[i].getAttribute("track_name");
        let spotifyTrackArtist = childLi[i].getAttribute("track_artist");

        let CreateTrack = {
            artist: spotifyTrackArtist,
            name: spotifyTrackName
        }
        listTracks.push(CreateTrack);
    }

    return JSON.stringify(listTracks);
}
function spotTest(displayName) {
    document.getElementById("user_id").innerText = displayName;
}
