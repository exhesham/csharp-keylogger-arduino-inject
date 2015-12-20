var sys = require("sys"),
my_http = require("http"),
url = require("url"),
fs = require('fs'),
qs = require('querystring');
var express = require('express');
var bodyParser = require('body-parser');
var app = express();
/// Include the express body parser
// parse application/x-www-form-urlencoded
app.use(bodyParser.urlencoded({ extended: true }))


UPDATES_FILE="updates.txt"
SESSIONS_FILE="sessions.txt"
function logToFile(filename,data)
{
	fs.appendFile(filename, data, function(err) {
		if(err) {
			
			return console.log(err);
		}

		console.log("data saved on "+filename);
	}); 
}



app.post('/dolphin_start', function(req, res) {
	logToFile(SESSIONS_FILE, JSON.stringify(req.body));
	res.send({ status: 'SUCCESS' });
});
app.post('/dolphin_new_data', function(req, res) {
	logToFile(UPDATES_FILE, JSON.stringify(req.body));
	res.send({ status: 'SUCCESS' });
});
app.get('/', function(req, res) {
	logToFile(UPDATES_FILE, JSON.stringify(req.body));
	res.write('<h1>exhesham\'s server - keep out</h1><p>This server was built for the dolphins - <b>if you are not a dolphin, then keep out please</p>' );
	res.send();
});


app.listen(12457);
sys.puts("Server Running on 12457"); 