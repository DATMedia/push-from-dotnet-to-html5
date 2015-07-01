var module = angular.module("eneterApp", []);

module.controller("mainController", ["$scope", function($scope) {
	$scope.welcomeMessage = "Hello Eneter App";
	
	var myBrokerClient;
	var myOutputChannel;
	
	 function onBrokerMessageReceived(brokerMessageReceivedEventArgs) {
                // If the notified message is the CPU status update.
                if (brokerMessageReceivedEventArgs.MessageTypeId === "MyCpuUpdate")
                {
                    // Deserialize notified message content.
                    var aValue = JSON.parse(brokerMessageReceivedEventArgs.Message);
                    
					$scope.cpuUsage = aValue.Usage;
					$scope.$apply();
                    // Update data and draw the chart.
//                    myLineChartData.datasets[0].data.shift();
//                    myLineChartData.datasets[0].data.push(aValue.Usage);
//                    myChart.Line(myLineChartData, myChartConfig);
                }
            };
	
	
	
	$scope.openConnection = function() {
		if(!myBrokerClient) {
			myBrokerClient = new DuplexBrokerClient();
			myBrokerClient.onBrokerMessageReceived = onBrokerMessageReceived;
			
		}
		myOutputChannel = myOutputChannel || new WebSocketDuplexOutputChannel("ws://127.0.0.1:8843/CpuUsage/", null);
		myBrokerClient.attachDuplexOutputChannel(myOutputChannel);
	};
	
	
	$scope.subscribe = function() {
        myBrokerClient.subscribe("MyCpuUpdate");
    };
	
	$scope.unsubscribe = function() {
	    myBrokerClient.unsubscribe("MyCpuUpdate");	
	};

	
	$scope.closeConnection = function() {
		myBrokerClient.detachDuplexOutputChannel();
	}
}]);