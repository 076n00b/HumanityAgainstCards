<?php
/*
 *	get.php
 *	Manatees Against Cars Global Server List
 *	Parameters:
 *		None
 *	Returns:
 *		JSON stream
 */

require_once("serverlist.php");

// Query server list
$serverList = new ServerList($database);

// Return it
echo(
	json_encode(
		$serverList->Get()
	)
);
