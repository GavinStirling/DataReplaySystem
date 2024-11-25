# Data Replay System

The aim of this project is to demonstrate a multi-threaded TCP server which generates stochastic data and streams it to various clients. The data is also stored in a basic SQLite database so that a separate server instance can replay sections of historical data.

## 1. Random Walk Server (Data Generation)

This is a multi-threaded TCP server which generates walking data. A random number is selected between 20.00 and 80.00 and increments the number by 0.01. The probability of the increment being positive or negative can be set. The increment can also be set depending on the value of the number. For example, if the number is greater than or equal to 60.00, then increment by 0,05 instead of 0.01. The server is multi-threaded so that multiple clients can connect to the server at the same time.

Once the number has been generated and stream, the number is stored in a basic SQLite server for later use.

## 2. Console Data Display

This is a client which connects to the server and displays the latest available number. The number is displayed in the console.

## 3. Data Display

This is a client which connects to the server and displays the latest available number in a Windows Form display. The latest available number is run in a background thread and then the UI is updated in the UI thread.

## 4. Time Plot Data Display

This is a client which first connects to the SQLite database and retrieves all of the historical data points. Once this is completed, the client connects to the server and waits for the latest available number. The data is then displayed using a scatter plot. Once the latest number is available, the plot is updated.

## 5. Random Walk Replay Server

This is a multi-threaded TCP server which accepts a start DateTime and an end DateTime. Using these starting and end points, the data for that time period is collected from the SQLite database and then streamed to any available clients. The idea is that the original data generated can be replayed from different times. The server is multi-threaded so that multiple clients can connect to the server at the same time.

## 6. Heat Map Data Display

This is a client which can consume data from various instances of the Random Walk Servers and the Random Walk Replay Servers at the same time. The Pearson correlation is then calculated and displayed in a Heat Map using Windows Forms.
