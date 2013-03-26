<?php
/*
 *	remove.php
 *	Manatees Against Cars Global Server List
 *	Remove a server from the global server list
 *
 *	Parameters:
 *		name			Display name of server
 *	Returns:
 *		JSON stream
 */

require_once('serverlist.php');

$name = @$_REQUEST['name'];

if (!isset($name) || trim($name) != $name)
{
	die(
		json_encode(
			array(
				'status'	=> 'failure',
				'reason'	=> 'Unsanitary input.'
			)
		)
	);
}

$serverList = new ServerList($database);

$result = $serverList->Remove($name);

if ($result == ServerList::ErrorSuccess)
{
	echo(
		json_encode(
			array(
				'status'		=> 'success'
			)
		)
	);
}
else
{
	$reason = 'Unknown';
	switch($result)
	{
		case ServerList::ErrorDatabase:
			$reason = 'Database error!';
			break;
		
		case ServerList::ErrorNoServer:
			$reason = 'No server with that name.';
			break;
	}
	
	echo(
		json_encode(
			array(
				'status'		=> 'failure',
				'reason'		=> $reason
			)
		)
	);
}
