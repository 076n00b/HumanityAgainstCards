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

// Grab variables
$name = @$_REQUEST['name'];

// Verify these are sanitary
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

// Allocate server list object
$serverList = new ServerList($database);

// Attempt to remove entry from server list
$result = $serverList->Remove($name);

if ($result == ServerList::ErrorSuccess)
{
	// Send successful result
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
	// Figure out what went wrong
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
	
	// Send failure result
	echo(
		json_encode(
			array(
				'status'		=> 'failure',
				'reason'		=> $reason
			)
		)
	);
}
