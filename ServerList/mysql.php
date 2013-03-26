<?php
/*
 * mysql.php
 * MySQL Driver
 */

class MySQL
{
	const MYSQL_SERVER   = '';
	const MYSQL_DATABASE = '';
	const MYSQL_USERNAME = '';
	const MYSQL_PASSWORD = '';
	
	private $mysqlHandle, $result;
	
	function __construct()
	{
		$this->mysqlHandle = mysql_connect(self::MYSQL_SERVER, self::MYSQL_USERNAME, self::MYSQL_PASSWORD);
		mysql_select_db(self::MYSQL_DATABASE);
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

?>
