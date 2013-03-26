<?php
/*
 *	serverlist.php
 *	Server List Abstraction
 */

require_once("mysql.php");

class ServerList
{
	const ErrorSuccess		= 0;
	const ErrorNameTaken 		= 1;
	const ErrorDatabase		= 2;
	const ErrorNoServer		= 3;
	
	private $database;
	
	function __construct($database)
	{
		$this->database = $database;
	}
	
	public function Get()
	{
		// Clean server list
		$this->CleanServerList();
		
		$statement = $this->database->prepare("SELECT `name`, `ipAddress` FROM `servers`");
		$statement->execute();
		
		if($statement->rowCount() == 0)
		{
			return array();
		}
		
		$servers = $statement->fetchAll(PDO::FETCH_ASSOC);
		$result = array();
		
		foreach($servers as $server)
		{
			$result[] = array(
				"name"		=> $server["name"],
				"ipAddress"	=> $server["ipAddress"]
			);
		}
		
		return $result;
	}
	
	public function Add($name, $ipAddress)
	{
		$statement = $this->database->prepare("SELECT * FROM `servers` WHERE `name` = :Name");
		$statement->execute(array(":Name" => $name));
		
		if($statement->rowCount() > 0)
		{
			return self::ErrorNameTaken;
		}
		
		$statement = $this->database->prepare("INSERT INTO `servers` (`name`, `ipAddress`, `lastHeartbeat`) VALUES (:Name, :Ip, :Time)");
		$result = $statement->execute(array(":Name" => $name, ":Ip" => $ipAddress, ":Time" => time()));
		
		return $result ? self::ErrorSuccess : self::ErrorDatabase;
	}
	
	public function Remove($name)
	{
		$statement = $this->database->prepare("DELETE FROM `servers` WHERE `name` = :Name LIMIT 1");
		$result = $statement->execute(array(":Name" => $name));
		
		if($result === true && $statement->rowCount() == 0)
		{
			return self::ErrorNoServer;
		}
		
		return $result ? self::ErrorSuccess : self::ErrorDatabase;
	}
	
	public function Heartbeat($name)
	{
		$statement = $this->database->prepare("SELECT * FROM `servers` WHERE `name` = :Name");
		$result = $statement->execute(array(":Name" => $name));
		
		$statement = $this->database->prepare("UPDATE `servers` SET `lastHeartbeat` = :Time WHERE `name` = :Name");
		$result = $statement->execute(array(":Time" => time(), ":Name" => $name));
		
		var_dump($result);
		
		if($result === true && $statement->rowCount() == 0)
		{
			return self::ErrorNoServer;
		}
		
		return $result ? self::ErrorSuccess : self::ErrorDatabase;
	}
	
	private function CleanServerList()
	{
		$statement = $this->database->prepare("DELETE FROM `servers` WHERE `lastHeartbeat` < unix_timestamp(NOW()) - 150");
		$result = $statement->execute();
		
		return $result ? self::ErrorDatabase : self::ErrorSuccess;
	}
}
