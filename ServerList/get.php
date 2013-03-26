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

$serverList = new ServerList($database);

echo(
	json_encode(
		$serverList->Get()
	)
);
