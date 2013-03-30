<?php
/*
 *	serverlist.php
 *	Server List Abstraction
 */

require_once("mysql.php");

class ServerList
{
	const ErrorSuccess		= 0;
	const ErrorNameTaken 	= 1;
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
		
		$statement = $this->database->prepare("SELECT `name`, `ipAddress`, `playerCount`, `passwordProtected` FROM `servers`");
		$statement->execute();
		
		if($statement->rowCount() == 0)
			return array();

		$servers = $statement->fetchAll(PDO::FETCH_ASSOC);
		$result = array();
		
		foreach($servers as $server)
		{
			$result[] = array(
				"name"				=> $server["name"],
				"ipAddress"			=> $server["ipAddress"],
				"playerCount"		=> (int)$server["playerCount"],
				"passwordProtected"	=> (boolean)$server["passwordProtected"]
			);
		}
		
		return $result;
	}
	
	public function Add($name, $ipAddress, $passwordProtected)
	{	
		$token = $this->GenerateToken();
		
		$this->CleanServerList();
		
		$statement = $this->database->prepare("SELECT * FROM `servers` WHERE `name` = :Name");
		$statement->execute(array(":Name" => $name));
		
		if($statement->rowCount() > 0)
			return array('errorCode' => ServerList::ErrorNameTaken);
		
		$statement = $this->database->prepare(
			"INSERT INTO `servers` (`name`, `ipAddress`, `lastHeartbeat`, `passwordProtected`, `playerCount`, `token`) " .
			"VALUES (:Name, :Ip, :Time, :PasswordProtected, 1, :Token)"
		);
		
		$result = $statement->execute(
			array(
				":Name" 				=> $name,
				":Ip" 					=> $ipAddress,
				":Time" 				=> time(),
				":PasswordProtected"	=> $passwordProtected,
				":Token"				=> $token
			)
		);
		
		return array(
			"errorCode"		=> $result ? ServerList::ErrorSuccess : ServerList::ErrorDatabase,
			"token"			=> $token
		);
	}
	
	public function Remove($token)
	{
		$statement = $this->database->prepare("DELETE FROM `servers` WHERE `token` = :Token LIMIT 1");
		$result = $statement->execute(array(":Token" => $token));
		
		if($result === true && $statement->rowCount() == 0)
			return ServerList::ErrorNoServer;
		
		return $result ? ServerList::ErrorSuccess : ServerList::ErrorDatabase;
	}
	
	public function Heartbeat($token)
	{
		$statement = $this->database->prepare("SELECT * FROM `servers` WHERE `token` = :Token");
		$result = $statement->execute(array(":Token" => $token));
		
		$statement = $this->database->prepare("UPDATE `servers` SET `lastHeartbeat` = :Time WHERE `token` = :Token");
		$result = $statement->execute(array(":Time" => time(), ":Token" => $token));
		
		if($result === true && $statement->rowCount() == 0)
			return ServerList::ErrorNoServer;
		
		return $result ? ServerList::ErrorSuccess : ServerList::ErrorDatabase;
	}
	
	public function Update($token, $playerCount)
	{
		$statement = $this->database->prepare("UPDATE `servers` SET `playerCount` = :PlayerCount WHERE `token` = :Token");
		$result = $statement->execute(array(":PlayerCount" => $playerCount, ":Token" => $token));
		
		if($result === true && $statement->rowCount() == 0)
			return ServerList::ErrorNoServer;
		
		return $result ? ServerList::ErrorSuccess : ServerList::ErrorDatabase;
	}
	
	private function CleanServerList()
	{
		$statement = $this->database->prepare("DELETE FROM `servers` WHERE `lastHeartbeat` < unix_timestamp(NOW()) - 150");
		$result = $statement->execute();
		
		return $result ? ServerList::ErrorDatabase : ServerList::ErrorSuccess;
	}
	
	private function GenerateToken()
	{
		// TODO Should make this a bit more unique
		return sha1(time());
	}
}
