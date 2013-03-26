<?php
/*
 *	serverlist.php
 *	Server List Abstraction
 */

require_once("mysql.php");

class ServerList
{
	const HeartbeatDelta	= 150;			// 2 1/2 minutes
	
	const ErrorSuccess		= 0;
	const ErrorNameTaken 	= 1;
	const ErrorDatabase		= 2;
	const ErrorNoServer		= 3;
	
	private $mysql;
	
	function __construct($mysql)
	{
		$this->mysql = $mysql;
	}
	
	public function Get()
	{
		$result = array();
		
		// Clean server list
		$this->CleanServerList();
		
		// Query DB for a list of servers
		$query = 'SELECT `name`, `ipAddress` FROM `servers`';
		$this->mysql->Query($query);
		
		// Are there any servers?
		if ($this->mysql->GetRowCount() == 0)
		{
			$this->mysql->Clean();
			return $result;
		}
		
		// Generate an entry for each server
		while($row = $this->mysql->FetchArray())
		{
			$result[] = array(
				'name'			=> $row['name'],
				'ipAddress'		=> $row['ipAddress']
			);
		}
		
		// Clean and return result
		$this->mysql->Clean();
		
		return $result;
	}
	
	public function Add($name, $ipAddress)
	{
		// Sanitize name
		$sanitizedName = $this->SterilizeString($name);
		$time = time();
		
		// Query DB for a list of servers (To verify we're not using someone else's name)
		$query = "SELECT `name`, `ipAddress` FROM `servers` WHERE `name` = '$sanitizedName'";
		$this->mysql->Query($query);
		
		// Are there any servers?
		if ($this->mysql->GetRowCount() != 0)
		{
			$this->mysql->Clean();
			return self::ErrorNameTaken;
		}
		
		// Clean this result
		$this->mysql->Clean();
		
		// Add server to list
		$query = "INSERT INTO `servers` (`name`, `ipAddress`, `lastHeartbeat`)" .
					"VALUES ('$sanitizedName', '$ipAddress', $time);";
		
		return !$this->mysql->Query($query) ? self::ErrorDatabase : self::ErrorSuccess;
	}
	
	public function Remove($name)
	{
		// Sanitize name
		$sanitizedName = $this->SterilizeString($name);
		
		// Query DB for a list of servers (To verify we're not using someone else's name)
		$query = "SELECT `name` FROM `servers` WHERE `name` = '$sanitizedName'";
		$this->mysql->Query($query);
		
		// Are there any servers?
		if ($this->mysql->GetRowCount() == 0)
		{
			$this->mysql->Clean();
			return self::ErrorNoServer;
		}
		
		$this->mysql->Clean();
		
		// Remove server from list
		$query = "DELETE FROM `servers` WHERE `name` = '$sanitizedName' LIMIT 1";
		return !$this->mysql->Query($query) ? self::ErrorDatabase : self::ErrorSuccess;
	}
	
	public function Heartbeat($name)
	{
		$sanitizedName = $this->SterilizeString($name);
		$time = time();
		
		$query = "UPDATE `servers` SET `lastHeartbeat` = '$time' WHERE `servers`.`name` = '$sanitizedName';";
		return !$this->mysql->Query($query) ? self::ErrorDatabase : self::ErrorSuccess;
	}
	
	private function CleanServerList()
	{
		// Get current time
		$time = time();
		
		// Query DB for a list of servers
		$query = 'SELECT `name`, `lastHeartbeat` FROM `servers`';
		$this->mysql->Query($query);
		
		// Are there any servers?
		if ($this->mysql->GetRowCount() == 0)
		{
			$this->mysql->Clean();
			return;
		}
		
		// Remove servers who's last heartbeat is over the delta
		while($row = $this->mysql->FetchArray())
		{
			// Skip servers who's heartbeats are inside of the delta
			if ($row['lastHeartbeat'] + self::HeartbeatDelta > $time)
				continue;
			
			// Bit of a hack
			$sanitizedName = $this->SterilizeString($row['name']);
			$removeQuery = "DELETE FROM `servers` WHERE `name` = '$sanitizedName' LIMIT 1";
			mysql_query($removeQuery);
		}
		
		// Clean result
		$this->mysql->Clean();
	}
	
	private function SterilizeString($value)
	{
		$sterilizedString = $value;
		
		$sterilizedString = trim($sterilizedString);
		$sterilizedString = stripslashes($sterilizedString);
		$sterilizedString = htmlspecialchars($sterilizedString);
		
		return $sterilizedString;
	}
}
