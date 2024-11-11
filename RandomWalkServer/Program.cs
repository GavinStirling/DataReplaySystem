using RandomWalkServer;

DbInstance db = new DbInstance();
TcpServer server = new TcpServer(5000, db);

    
