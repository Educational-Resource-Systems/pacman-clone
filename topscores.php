<?php
// Database configuration
$host = "localhost"; // Update with your database host
$username = "ersdev_pacman"; // Update with your database username
$password = "fish007"; // Update with your database password
$database = "ersdev_pacmangame"; // Update with your database name

// Create database connection
$conn = new mysqli($host, $username, $password, $database);

// Check connection
if ($conn->connect_error) {
    echo "DATABASE CONNECTION FAILED";
    exit();
}

// Query to fetch top 10 highscores, ordered by score descending
$sql = "SELECT name, score FROM highscores ORDER BY score DESC LIMIT 10";
$result = $conn->query($sql);

if ($result === false) {
    echo "QUERY FAILED";
    $conn->close();
    exit();
}

// Check if any results were returned
if ($result->num_rows > 0) {
    // Output scores in the format: name\tscore\n
    while ($row = $result->fetch_assoc()) {
        // Sanitize name to prevent injection or formatting issues
        $name = htmlspecialchars($row['name'], ENT_QUOTES, 'UTF-8');
        echo $name . "\t" . $row['score'] . "\n";
    }
} else {
    echo "NO SCORES FOUND";
}

// Close the connection
$conn->close();
?>