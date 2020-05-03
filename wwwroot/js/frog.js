
const square = document.querySelectorAll('.frog-square');
const frog = document.querySelectorAll('.frog');

const timeLeft = document.querySelector('#time-left');
let score = document.querySelector('#result');

let result = 0;
let currentTime = timeLeft.textContent;
var canClick = true;
var gameOver = false;


function randomSquare() {
    canClick = true;
    square.forEach(cn => {
        cn.classList.remove('frog');
        cn.classList.remove('frozen');

    })
    let randomPosition = square[Math.floor(Math.random() * 9)];
    randomPosition.classList.add('frog');
    hitPosition = randomPosition.id;

}

function preloadImage(url) {
    var img = new Image();
    img.src = url;
}

square.forEach(id => {
    id.addEventListener('mouseup', () => {
        if (id.id === hitPosition) {
            if (canClick) {
                id.classList.remove('frog')
                id.classList.add('frozen')
                canClick = false;
                result++;
                score.textContent = result;
            }
        }
    })
})

function moveFrog() {
    preloadImage('/img/frog/frog.png')
    preloadImage('/img/frog/frog-frozen.png')

    let timerId = null
    loop();

}

function loop() {
    if (!gameOver) {
        
        var rand = 1000;
        setTimeout(function () {
            randomSquare();
            loop();
        }, rand);
    }

};

moveFrog();

function countDown() {
    currentTime--
    timeLeft.textContent = currentTime;
    if (currentTime === 0) {
        gameOver = true;
        alert('game over! score: ' + result)
        location.reload()
    }
}

let timerId = setInterval(countDown, 1000);


