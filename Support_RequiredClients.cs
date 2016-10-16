// Protocol:
//
// Client connects to server
// Server rejects connection, tells what clients are required
// Client

$RC::Debug = true;

if($RC::Version >= 1 && !$RC::Debug)
  return;

$RC::Version = 1;

$RC::Error[1] = "Non-Glass Add-On";
$RC::Error[2] = "Already Registered";
$RC::Error[3] = "Jettison Error";
$RC::Error[4] = "Glass is in use";

function registerClient() {
  //filler function
  return 4;
}

if(isFile("Add-Ons/System_BlocklandGlass/client.cs"))
  return; //Glass handles this

if(!isFunction(jettisonParse)) {
  if(isFile("./jettison.cs")) {
    exec("./jettison.cs");
  } else if(isFile("./Support_Jettison.cs")) {
    exec("./Support_Jettison.cs");
  } else if(isFile($RC::JettisonLocation)) {
    exec($RC::JettisonLocation);
  } else {
    error("Support_RequiredClients: Failed to find jettison! Please refer to documentation");
    return;
  }
}

function registerClient(%addonName) {
  if($RC::RegisteredName[%addonName])
    return 2;

  if(!isFile("Add-Ons/" @ %addonName @ "/glass.json"))
    return 1;

  %error = jettisonReadFile("Add-Ons/" @ %addonName @ "/glass.json");
  if(%error) {
    return 3;
  }

  %obj = $JSON::Value;

  %id = %obj.value["id"];

  if($RC::RegisteredId[%id])
    return 2;

  $RC::HasClient[%id] = true;
  $RC::ClientIds = trim($RC::ClientIds TAB %id);

  $RC::RegisteredName[%addonName] = true;
  $RC::RegisteredId[%id] = true;
}

function RC::handleRequest(%msg) { // takes in the connection rejection message from the server, builds the reconnect str to tell the server we have it
  %optional = (getField(%msg, 0) $= "MISSING_OPT");

  echo("Optional: " @ (%optional ? "true" : "false"));

  %requested = trim(setField(%msg, 0, ""));
  %reconStr = "";

  $RC::MissingCt = 0;

  for(%i = 0; %i < getFieldCount(%requested); %i++) {
    %args = strreplace(getField(%requested, %i), "^", "\t");
    %name = getField(%args, 0);
    %reqId = getField(%args, 1);

    echo("Requested \"" @ %name @ "\" (" @ %reqId @ ")");

    if($RC::HasClient[%reqId]) {
      %reconStr = trim(%reconStr SPC %reqId);
    } else {
      echo("missing");
      $RC::MissingName[$RC::MissingCt+0] = %name;
      $RC::MissingId[$RC::MissingCt+0] = %reqId;
      $RC::MissingCt++;
    }
  }

  echo("MissingCt: " @ $RC::MissingCt);
  if($RC::MissingCt > 0) {
    RC::displayMissing(%optional);
    return;
  }

  $RC::ConnectStr = %reconStr;

  RC::doReconnect();
}

function RC::doReconnect() {
  connectToServer($RC::PrevConnect[%c=0], $RC::PrevConnect[%c++], $RC::PrevConnect[%c++], $RC::PrevConnect[%c++]);
}

function RC::joinAnyway() {
  $RC::Override = true;
  RC::doReconnect();
}

function RC::displayMissing(%opt) {
  if(%opt) {
    %str = "It's recommended that you have the following client add-ons to join this server:<br><br>";
  } else {
    %str = "You are required to have the following client add-ons to join this server:<br><br>";
  }


  for(%i = 0; %i < $RC::MissingCt; %i++) {
    %str = %str @ "<a:blocklandglass.com/addons/addon.php?id=" @ $RC::MissingId[%i] @ ">" @ $RC::MissingName[%i] @ "</a><br>";
  }
  %str = %str @ "<br>Alternatively, you can download <a:blocklandglass.com/dl.php>Blockland Glass</a> to manage client add-ons automatically.";

  if(%opt) {
    %str = %str @ "<br><br>Do you want to join anyway?";
    messageBoxYesNo("Missing Clients", %str, "RC::joinAnyway();");
  } else {
    messageBoxOk("Missing Clients", %str);
  }
}

package RequiredClients {
  function connectToServer(%addr, %pass, %a, %b) {
    $RC::PrevConnect[%c=0] = %addr;
    $RC::PrevConnect[%c++] = %pass;
    $RC::PrevConnect[%c++] = %a;
    $RC::PrevConnect[%c++] = %b;
    parent::connectToServer(%addr, %pass, %a, %b);
  }

  function GameConnection::onConnectRequestRejected(%this, %reason) {
    if(getField(%reason, 0) $= "MISSING" || getField(%reason, 0) $= "MISSING_OPT") {
      canvas.popDialog(connectingGui);
      RC::handleRequest(%reason);
    } else {
      parent::onConnectRequestRejected(%this, %reason);
    }
  }

  function GameConnection::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, %h, %i, %j, %k, %l, %m, %n, %o,%p) {
    parent::setConnectArgs(%a, %b, %c, %d, %e, %f, %g, "ReqCli" TAB $RC::ConnectStr TAB $RC::Override NL %h, %i, %j, %k, %l, %m, %n, %o, %p);
    $RC::Override = false;
  }
};
activatePackage(RequiredClients);
