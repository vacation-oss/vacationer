const canvas = document.getElementById("game");
const ctx = canvas.getContext("2d");
const scoreEl = document.getElementById("score");
const levelEl = document.getElementById("level");
const linesEl = document.getElementById("lines");
const messageEl = document.getElementById("message");
const startBtn = document.getElementById("startBtn");

const COLS = 10;
const ROWS = 20;
const BLOCK = 30;
const EMPTY = 0;

const SHAPES = {
  I: [[1, 1, 1, 1]],
  O: [
    [2, 2],
    [2, 2],
  ],
  T: [
    [0, 3, 0],
    [3, 3, 3],
  ],
  S: [
    [0, 4, 4],
    [4, 4, 0],
  ],
  Z: [
    [5, 5, 0],
    [0, 5, 5],
  ],
  J: [
    [6, 0, 0],
    [6, 6, 6],
  ],
  L: [
    [0, 0, 7],
    [7, 7, 7],
  ],
};

const COLORS = {
  0: "#0b0e15",
  1: "#66d9ef",
  2: "#f6d365",
  3: "#c792ea",
  4: "#7bd88f",
  5: "#ff7a90",
  6: "#82aaff",
  7: "#ffcb6b",
};

const SCORE_TABLE = [0, 100, 300, 500, 800];

let board = [];
let piece = null;
let score = 0;
let lines = 0;
let level = 1;
let gameOver = false;
let paused = false;
let lastTime = 0;
let dropCounter = 0;

function createBoard() {
  return Array.from({ length: ROWS }, () => Array(COLS).fill(EMPTY));
}

function randomPiece() {
  const types = Object.keys(SHAPES);
  const type = types[Math.floor(Math.random() * types.length)];
  const shape = SHAPES[type].map((row) => [...row]);
  return {
    x: Math.floor((COLS - shape[0].length) / 2),
    y: 0,
    shape,
  };
}

function rotate(matrix) {
  return matrix[0].map((_, i) => matrix.map((row) => row[i]).reverse());
}

function collide(testPiece) {
  return testPiece.shape.some((row, y) =>
    row.some((value, x) => {
      if (value === EMPTY) return false;
      const newX = testPiece.x + x;
      const newY = testPiece.y + y;
      return newX < 0 || newX >= COLS || newY >= ROWS || (newY >= 0 && board[newY][newX] !== EMPTY);
    })
  );
}

function merge() {
  piece.shape.forEach((row, y) => {
    row.forEach((value, x) => {
      if (value !== EMPTY) {
        board[piece.y + y][piece.x + x] = value;
      }
    });
  });
}

function clearLines() {
  let cleared = 0;
  for (let y = ROWS - 1; y >= 0; y--) {
    if (board[y].every((v) => v !== EMPTY)) {
      board.splice(y, 1);
      board.unshift(Array(COLS).fill(EMPTY));
      cleared += 1;
      y += 1;
    }
  }

  if (cleared > 0) {
    lines += cleared;
    score += SCORE_TABLE[cleared] * level;
    level = Math.floor(lines / 10) + 1;
    updateStats();
  }
}

function updateStats() {
  scoreEl.textContent = String(score);
  levelEl.textContent = String(level);
  linesEl.textContent = String(lines);
}

function drawCell(x, y, value) {
  ctx.fillStyle = COLORS[value];
  ctx.fillRect(x * BLOCK, y * BLOCK, BLOCK, BLOCK);
  ctx.strokeStyle = "#101827";
  ctx.strokeRect(x * BLOCK, y * BLOCK, BLOCK, BLOCK);
}

function draw() {
  ctx.clearRect(0, 0, canvas.width, canvas.height);

  board.forEach((row, y) => {
    row.forEach((value, x) => drawCell(x, y, value));
  });

  if (piece) {
    piece.shape.forEach((row, y) => {
      row.forEach((value, x) => {
        if (value !== EMPTY) {
          drawCell(piece.x + x, piece.y + y, value);
        }
      });
    });
  }
}

function spawnPiece() {
  piece = randomPiece();
  if (collide(piece)) {
    gameOver = true;
    messageEl.textContent = "게임 오버! 시작 버튼으로 다시 시작하세요.";
    startBtn.textContent = "다시 시작";
  }
}

function hardDrop() {
  while (!collide({ ...piece, y: piece.y + 1 })) {
    piece.y += 1;
  }
  tick();
}

function tick() {
  if (gameOver || paused) return;
  const test = { ...piece, y: piece.y + 1 };
  if (!collide(test)) {
    piece = test;
    return;
  }

  merge();
  clearLines();
  spawnPiece();
}

function update(time = 0) {
  if (!gameOver) {
    const delta = time - lastTime;
    lastTime = time;

    if (!paused) {
      dropCounter += delta;
      const speed = Math.max(100, 700 - (level - 1) * 50);
      if (dropCounter > speed) {
        tick();
        dropCounter = 0;
      }
    }

    draw();
    requestAnimationFrame(update);
  } else {
    draw();
  }
}

function startGame() {
  board = createBoard();
  score = 0;
  lines = 0;
  level = 1;
  gameOver = false;
  paused = false;
  dropCounter = 0;
  lastTime = 0;
  updateStats();
  spawnPiece();
  messageEl.textContent = "게임 진행 중... P 키로 일시정지";
  startBtn.textContent = "재시작";
  requestAnimationFrame(update);
}

document.addEventListener("keydown", (e) => {
  if (!piece || gameOver) return;

  if (e.key === "p" || e.key === "P") {
    paused = !paused;
    messageEl.textContent = paused ? "일시정지됨" : "게임 진행 중... P 키로 일시정지";
    return;
  }

  if (paused) return;

  if (e.key === "ArrowLeft") {
    const next = { ...piece, x: piece.x - 1 };
    if (!collide(next)) piece = next;
  } else if (e.key === "ArrowRight") {
    const next = { ...piece, x: piece.x + 1 };
    if (!collide(next)) piece = next;
  } else if (e.key === "ArrowDown") {
    tick();
  } else if (e.key === "ArrowUp") {
    const rotated = { ...piece, shape: rotate(piece.shape) };
    if (!collide(rotated)) piece = rotated;
  } else if (e.code === "Space") {
    hardDrop();
  }

  draw();
});

startBtn.addEventListener("click", startGame);

board = createBoard();
draw();
