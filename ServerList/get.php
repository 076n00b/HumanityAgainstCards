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
$mysql = new MySQL();
$serverList = new ServerList($mysql);

// Return it
echo(
	json_encode(
		$serverList->Get()
	)
);

?>
