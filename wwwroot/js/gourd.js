document.addEventListener('DOMContentLoaded', () => {
    const squares = document.querySelectorAll('.gourd-grid div');
    const scoreDisplay = document.querySelector('.score');
    const startButton = document.querySelector('.start');

    const width = 10;
    let currentIndex = 0;
    let gourdIndex = 0;
    //2 - head, 0 - tail
    let currentsuika = [2, 1, 0];
    let direction = 1;
    let score = 0;
    let speed = 50;
    let intervalTime = 0;
    let interval = 0;

    function control(e) {
        squares[currentIndex].classList.remove('suika')

        if (e.keyCode === 39) {
            direction = 1;
        } else if (e.keyCode === 38) {
            direction = -width;
        } else if (e.keyCode === 37) {
            direction = -1;
        } else if (e.keyCode === 40) {
            direction = +width;
        }
    }

    function randomgourd() {
        do {
            gourdIndex = Math.floor(Math.random() * squares.length)
        } while (squares[gourdIndex].classList.contains('suika')) //making sure gourds dont appear on the suika
        squares[gourdIndex].classList.add('gourd')
    }

    function startGame() {
        currentsuika.forEach(index => squares[index].classList.remove('suika'))
        squares[gourdIndex].classList.remove('gourd')
        clearInterval(interval)
        score = 0;
        randomgourd()
        direction = 1;
        scoreDisplay.textContent = score;
        intervalTime = 800;
        currentsuika = [2, 1, 0]
        currentIndex = 0;
        currentsuika.forEach(index => squares[index].classList.add('suika'))
        interval = setInterval(moveOutcomes, intervalTime)
    }



    function moveOutcomes() {

        if (
            (currentsuika[0] + width >= (width * width) && direction === width) || //bottom
            (currentsuika[0] % width === width - 1 && direction === 1) || //right
            (currentsuika[0] % width === 0 && direction === -1) || //left
            (currentsuika[0] - width < 0 && direction === -width) ||  //top
            squares[currentsuika[0] + direction].classList.contains('suika') //self
        ) {
            scoreDisplay.textContent = ' Final score ' + score;

            return clearInterval(interval)
        }

        const tail = currentsuika.pop();
        squares[tail].classList.remove('suika');
        currentsuika.unshift(currentsuika[0] + direction)

        if (squares[currentsuika[0]].classList.contains('gourd')) {
            squares[currentsuika[0]].classList.remove('gourd')
            squares[tail].classList.add('suika')
            currentsuika.push(tail)
            randomgourd()
            score++;
            scoreDisplay.textContent = score;
            clearInterval(interval)
            intervalTime = intervalTime - speed;
            if (intervalTime < 100) intervalTime = 100;
            interval = setInterval(moveOutcomes, intervalTime);
        }
        squares[currentsuika[0]].classList.add('suika')
    }

    document.addEventListener('keyup', control)
    startButton.addEventListener('click', startGame)

});