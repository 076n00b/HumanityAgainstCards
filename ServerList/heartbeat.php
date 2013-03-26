<?php
/*
 *	heartbeat.php
 *	Manatees Against Cars Global Server List
 *	Update the last heartbeat for a server
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
$mysql = new MySQL();
$serverList = new ServerList($mysql);

// Attempt to heartbeat server on server list
$result = $serverList->Heartbeat($name);

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

?>
