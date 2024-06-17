# Load Balancer (lb)

## Overview

This source code is intended as a suggested approach to a coding challenge posed at:

https://codingchallenges.fyi/challenges/challenge-load-balancer/

## Approach

A C# application designed using SOLID principles, facilitating extensibility and testing of behivours.  This version uses the asyn/await pattern allowing asynchronous, non-blocking function calls.

## Configuration

The load balancer can be configured to use following backend web service selection algorithms:

* Weighted Round Robin
* Round Robin

    ### appsetting.json example:

    ```json
    {
      "LoadBalanceOptions": {
        "HealthCheckDelay": 10000,
        "Port": 443,
        "LoadBalancerSelector": "WeightedRoundRobinSelector" // or "RoundRobinSelector", etc.
      }
    }
    ```
 
    Here is a suggested set of backend web services that the load balancer will apply the above selection algorithms to:
    
    ```json
    {
      "BackendServers": [
        {
          "HostName": "localhost",
          "Port": 7076,
          "IsHealthy": false,
          "Weight": 1
        },
        {
          "HostName": "localhost",
          "Port": 7077,
          "IsHealthy": false,
          "Weight": 2
        },
        {
          "HostName": "localhost",
          "Port": 7078,
          "IsHealthy": false,
          "Weight": 5
        }
      ]
    }
    ```

    NOTE: Please feel free to change the port settings and host names appropriately. 


    ### Serilog

    Currently setup using the following format:

    ```json
        "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{Application}][{MachineName}][{ThreadId}] {Message:lj}{NewLine}{Exception}"
    ```

    Logs are currenly dumped in ./logs/log-.txt".  Refer to the json config file for further details.


## Running the Load Balancer

As suggested in the coding challenge:
* create several instances of the python web service (command provided in Step 2).
* use Curl as a client request, I had to use something like this:
 
```console
    curl --http0.9 http://localhost/443
```
* check the logs for expected output.

