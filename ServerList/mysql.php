<?php
/*
 * mysql.php
 * MySQL Driver
 */

require_once("config.php");

class MySQL
{
	private $mysqlHandle, $result;
	
	function __construct()
	{
		global $mysql_config;
		$this->mysqlHandle = mysql_connect($mysql_config["host"], $mysql_config["username"], $mysql_config["password"]);
		mysql_select_db($mysql_config["database"]);
	}
	
	function __destruct()
	{
		mysql_close($this->mysqlHandle);
	}
	
	public function Query($value)
	{
		$this->result = mysql_query($value);
		return $this->result;
	}
	
	public function Clean()
	{
		mysql_free_result($this->result);
	}
	
	public function GetRowCount()
	{
		return mysql_num_rows($this->result);
	}
	
	public function FetchArray()
	{
		return mysql_fetch_array($this->result);
	}
}
