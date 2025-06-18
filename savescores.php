<?php
// Database configuration
$host = "localhost";
$username = "ersdev_pacman";
$password = "fish007";
$database = "ersdev_pacmangame";

// Create database connection
$conn = new mysqli($host, $username, $password, $database);

// Check connection
if ($conn->connect_error) {
    echo json_encode(["status" => "error", "message" => "DATABASE CONNECTION FAILED"]);
    exit();
}

// Get POST data
$name = isset($_POST['name']) ? $_POST['name'] : '';
$score = isset($_POST['score']) ? (int)$_POST['score'] : 0;

// Validate input
if (empty($name) || $score <= 0) {
    echo json_encode(["status" => "error", "message" => "INVALID INPUT"]);
    $conn->close();
    exit();
}

// Sanitize name
$name = $conn->real_escape_string($name);

// Insert new score
$sql = "INSERT INTO highscores (name, score) VALUES ('$name', $score)";
if ($conn->query($sql) === TRUE) {
    echo json_encode(["status" => "success"]);
} else {
    echo json_encode(["status" => "error", "message" => $conn->error]);
}

// Close the connection
$conn->close();
?>