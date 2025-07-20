function spotifyShowPlaylists(bShowPlaylists) {

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
        spotifyResetGenerator();
    }
}

//function spotifySetSearchDisplay(jsonDataUser, jsonListPlaylists) {

//    spotifyDisplayUserInfo(jsonDataUser);
//    spotifyDisplayPlaylists(jsonListPlaylists);
//}
//function spotifyDisplayUserInfo(jsonDataUser) {
//    //console.debug("spotifyDisplayUserInfo");

//    const obj = JSON.parse(jsonDataUser);
//    document.getElementById("user_id").innerText = obj.SpotifyID;
//}
function spotifyDisplayNavigation() {
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
    document.getElementById("input_Prompt").style.display = "none";
    document.getElementById("input_PlaylistName").style.display = "";
    document.getElementById("btn_CreatePlaylist").style.display = "";
    document.getElementById("btn_SelectAll").style.display = "";

    let obj = JSON.parse(jsonData);
    document.getElementById("input_Prompt").value = "";

    document.getElementById("ai_PlaylistDescription").innerText = obj.Description;

    for (var index in obj.Playlist) {
        //console.debug(`spotifyProcessChatResponse adding: ${obj.Playlist[index].Title} - ${obj.Playlist[index].Artist}.`);
        let elmLi = document.createElement("li");
        let elmInput = document.createElement("input");
        let elmA = document.createElement("a");

        elmLi.setAttribute("spotify_id", obj.Playlist[index].Spotify_Id);
        elmLi.style.listStyleType = "none";

        elmInput.classList.add("w3-check");
        elmInput.type = "checkbox";
        elmInput.id = `check_${obj.Playlist[index].Spotify_Id}`;
        elmInput.checked = true;

        elmA.innerText = `${obj.Playlist[index].Title} - ${obj.Playlist[index].Artist}`;

        elmLi.appendChild(elmInput);
        elmLi.appendChild(elmA);
        
        aiListPlaylistTracks.appendChild(elmLi);
    }

    const arrIDsToHide = ["display_Loading", "spotifyPlaylistDisplay", "spotifyPlaylistsDisplay"];

    for (let i = 0; i < arrIDsToHide.length; i++) {
        document.getElementById(arrIDsToHide[i]).style.display = "none";
    }
    aiGeneratedPlaylist.style.display = "";
}

function spotifyShowLoading() {
    document.getElementById("display_Loading").style.display = "";
    document.getElementById("spotifyPlaylistDisplay").style.display = "none";
    document.getElementById("spotifyPlaylistsDisplay").style.display = "none";
    document.getElementById("input_Prompt").style.display = "none";
    document.getElementById("btn_SubmitPrompt").style.display = "none";
}

function spotifyGetTracks() {

    let listTracks = [];

    const aiListPlaylistTracks = document.getElementById("ai_ListPlaylistTracks");
    const childLi = aiListPlaylistTracks.getElementsByTagName("li");

    for (let i = 0; i < childLi.length; i++) {
        let spotifyId = childLi[i].getAttribute("spotify_id");
        let checkbox = document.getElementById(`check_${spotifyId}`);

        if (checkbox.checked == true) {
            listTracks.push(spotifyId);
        }
    }

    return JSON.stringify(listTracks);
}

function spotifyCreatePlaylistDisplay() {

    document.getElementById("aiPrompts").style.display = "none";
    document.getElementById("ai_GeneratedPlaylist").style.display = "none";
    document.getElementById("display_CreatingPlaylist").style.display = "";
    document.getElementById("ai_PlaylistDescription").innerHTML = "";
    document.getElementById("ai_ListPlaylistTracks").innerHTML = "";
}

//function spotifySetCreatedPlaylistDisplay() {
//    //spotifyResetGenerator();
//    document.getElementById("display_CreatingPlaylist").style.display = "none";
//}

function spotifySelectAll() {
    const aiListPlaylistTracks = document.getElementById("ai_ListPlaylistTracks");
    const childLi = aiListPlaylistTracks.getElementsByTagName("li");

    for (let i = 0; i < childLi.length; i++) {
        let spotifyId = childLi[i].getAttribute("spotify_id");
        let checkbox = document.getElementById(`check_${spotifyId}`);
        checkbox.checked = true;
    }
}

function spotifyResetGenerator() {
    console.debug("spotifyResetGenerator called");
    const aiPrompts = document.getElementById("aiPrompts");
    const inputPrompt = document.getElementById("input_Prompt");
    const inputPlaylistName = document.getElementById("input_PlaylistName");
    const btnSubmitPrompt = document.getElementById("btn_SubmitPrompt");
    const btnSelectAll = document.getElementById("btn_SelectAll");
    const btnCreatePlaylist = document.getElementById("btn_CreatePlaylist");
    const displayLoading = document.getElementById("display_Loading");
    const displayCreatingPlaylist = document.getElementById("display_CreatingPlaylist");
    const aiGeneratedPlaylist = document.getElementById("ai_GeneratedPlaylist");
    const aiPlaylistDescription = document.getElementById("ai_PlaylistDescription");
    const aiListPlaylistTracks = document.getElementById("ai_ListPlaylistTracks");

    inputPrompt.value = "";
    inputPlaylistName.value = "";
    inputPrompt.style.display = "";
    btnSubmitPrompt.style.display = "";
    aiPlaylistDescription.innerHTML = "";
    aiListPlaylistTracks.innerHTML = "";

    let arrAiElm = [aiPrompts, inputPlaylistName, btnSelectAll, btnCreatePlaylist, displayLoading, displayCreatingPlaylist, aiGeneratedPlaylist];
    for (var i = 0; i < arrAiElm.length; i++) {
        arrAiElm[i].style.display = "none";
    }

    
}
//function spotTest(displayName) {
//    document.getElementById("user_id").innerText = displayName;
//}
