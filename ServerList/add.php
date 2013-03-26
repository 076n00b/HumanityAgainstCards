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

// Grab variables
$name = @$_REQUEST['name'];
$ipAddress = @$_SERVER['REMOTE_ADDR'];

// Verify these are sanitary
if (!isset($name) || !isset($ipAddress) ||
	trim($name) != $name)
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

// Attempt to add entry to server list
$result = $serverList->Add($name, $ipAddress);

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
		case ServerList::ErrorNameTaken:
			$reason = 'Server of that name already exists!';
			break;
		
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
