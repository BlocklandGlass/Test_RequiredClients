//Test script for Support_RequiredClients

exec("./Support_RequiredClients.cs");

%error = registerClient("Test_RequiredClients");

if(%error) {
  warn("Error Registering Required Client: \c2" @ $RC::Error[%error]);
} else {
  echo("Succesfully Registered Client");
}
