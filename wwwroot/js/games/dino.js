document.addEventListener('DOMContentLoaded', () => {
    const dino = document.querySelector('.dino')
    const grid = document.querySelector('.grid')
    const timeLabel = document.querySelector('#time')
    const startButton = document.querySelector('.start-button')
    let canJump = true
    let gravity = 0.9
    let isGameOver = false
    let obstacleSpeed = 10;
    let time = 0;
    let timerInterval;
    let arr = [];
    let timeout;

    function control(e) {
        if (e.keyCode === 32) {
            if (canJump && !isGameOver) {
                canJump = false
                jump()
            }
        }
    }
    document.addEventListener('keydown', control)

    let position = 0
    function jump() {
        let acceleration = 20;
        let timerId = setInterval(function () {

            //move up
            position += acceleration
            acceleration = acceleration - gravity
            dino.style.bottom = position + 'px'

            if (position < 5) {
                position = 5;
                dino.style.bottom = position + 'px'
                canJump = true;
                clearInterval(timerId);
            }
        }, 20)
    }

    function generateObstacles() {
        if (!isGameOver) {
            let randomTime = 600 - obstacleSpeed + Math.random() * 1750
            timeout = setTimeout(generateObstacles, randomTime)

        let obstaclePosition = window.screen.width - 65;
        let obstacle = document.createElement('div')
         obstacle.classList.add('obstacle')
        grid.appendChild(obstacle)
        obstacle.style.left = obstaclePosition + 'px'

        let timerId = setInterval(function () {
            if (obstaclePosition > 120 && obstaclePosition < 180 && position < 60) {
                arr.map((a) => {
                    clearInterval(a);
                    arr = [];
                })
                clearInterval(timerId);
                canJump = false;
                isGameOver = true
                clearInterval(timerInterval);
                clearTimeout(timeout);
                dino.classList.remove("running");
                dino.classList.add("dino-dead");
                startButton.style.display = "block";
            }
            obstaclePosition -= obstacleSpeed
            obstacle.style.left = obstaclePosition + 'px'
            if (obstaclePosition < 0) {
                obstacleSpeed += 1;
                clearInterval(timerId);
                grid.removeChild(obstacle);
            }
        }, 20);
        arr.push(timerId);
        }
    }

    startButton.onclick = function () {
        isGameOver = false;
        canJump = true;
        obstacleSpeed = 10;
        time = 0;
        let obstacles = grid.querySelectorAll(".obstacle")

        obstacles.forEach(element => grid.removeChild(element));

        dino.classList.remove("dino-dead");
        dino.classList.add("running");
        timeLabel.style.display = "visible";
        timerInterval = setInterval(function () {
            time += 0.01;
            timeLabel.innerHTML = Math.round(time * 100) / 100 + " s.";
        }, 10)
        generateObstacles();
        startButton.style.display = "none";
    }

    var interval_id = window.setInterval("", 9999); // Get a reference to the last
    // interval +1
    for (var i = 1; i < interval_id; i++)
        window.clearInterval(i);

})