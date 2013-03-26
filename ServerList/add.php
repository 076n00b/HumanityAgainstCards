<?php
/*
 *	add.php
 *	Manatees Against Cars Global Server List
 *	Add a server to the global server list
 *
 *	Parameters:
 *		name			Display name of server
 *	Returns:
 *		JSON stream
 */

require_once('serverlist.php');

if(empty($_GET['name']))
{
	echo(json_encode(array(
		"status"	=> "failure",
		"reason"	=> "No name provided."
	)));
	
	die();
}

$serverList = new ServerList($database);

$result = $serverList->Add($_GET['name'], $_SERVER['REMOTE_ADDR']);

if ($result == ServerList::ErrorSuccess)
{
	echo(json_encode(array(
		'status'	=> 'success'
	)));
}
else
{
	$reason = 'Unknown';
	
	switch($result)
	{
		case ServerList::ErrorNameTaken:
			$reason = 'Server with that name already exists.';
			break;
		
		case ServerList::ErrorDatabase:
			$reason = 'Database error!';
			break;
	}
	
	echo(json_encode(array(
		'status'	=> 'failure',
		'reason'	=> $reason
	)));
}
