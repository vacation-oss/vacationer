// =============================
// 2D 건물 부수기 게임 (초보자용)
// - Matter.js 물리엔진 사용
// - 파일: index.html / style.css / script.js
// =============================

// Matter.js에서 자주 쓰는 기능을 꺼내옵니다.
const { Engine, World, Bodies, Body, Mouse, MouseConstraint, Events } = Matter;

// HTML 요소를 가져옵니다.
const canvas = document.getElementById('gameCanvas');
const ctx = canvas.getContext('2d');
const ammoEl = document.getElementById('ammo');
const scoreEl = document.getElementById('score');
const blocksLeftEl = document.getElementById('blocksLeft');
const messageEl = document.getElementById('message');
const restartBtn = document.getElementById('restartBtn');

// -----------------------------
// [튜닝 포인트] 초보자가 수정하기 쉬운 설정값들
// -----------------------------
const CONFIG = {
  // 중력 크기 (숫자가 클수록 더 빨리 떨어짐)
  gravityY: 1.0,

  // 포탄 개수
  maxAmmo: 6,

  // 건물 블록 층 수 / 열 수
  blockRows: 6,
  blockCols: 5,

  // 블록 크기
  blockWidth: 44,
  blockHeight: 28,

  // 포탄 크기
  projectileRadius: 14,

  // 발사대(새총) 위치
  launcherX: 145,
  launcherY: 420,

  // 마우스로 당길 수 있는 최대 거리
  maxDragDistance: 140,
};

// 물리 엔진 생성
const engine = Engine.create();
engine.gravity.y = CONFIG.gravityY;

// 게임 상태값
let ammo = CONFIG.maxAmmo;
let score = 0;
let stageCleared = false;
let gameOver = false;

// 월드 오브젝트 저장용
let worldBodies = [];
let blocks = [];
let currentProjectile = null;
let mouseConstraint = null;
let isDraggingProjectile = false;

// 고정 바닥과 벽을 생성합니다.
function createBoundaries() {
  const floor = Bodies.rectangle(500, 548, 1000, 24, { isStatic: true, label: 'floor' });
  const leftWall = Bodies.rectangle(-10, 280, 20, 560, { isStatic: true, label: 'wall' });
  const rightWall = Bodies.rectangle(1010, 280, 20, 560, { isStatic: true, label: 'wall' });

  World.add(engine.world, [floor, leftWall, rightWall]);
  worldBodies.push(floor, leftWall, rightWall);
}

// 직사각형 블록으로 건물을 만듭니다.
function createBuilding() {
  blocks = [];

  const startX = 720;
  const startY = 510;

  for (let row = 0; row < CONFIG.blockRows; row += 1) {
    for (let col = 0; col < CONFIG.blockCols; col += 1) {
      const x = startX + col * (CONFIG.blockWidth + 2);
      const y = startY - row * (CONFIG.blockHeight + 2);

      const block = Bodies.rectangle(x, y, CONFIG.blockWidth, CONFIG.blockHeight, {
        restitution: 0.1, // 튕김 정도
        friction: 0.7, // 마찰력
        density: 0.0024, // 밀도
        label: 'building-block',
      });

      blocks.push(block);
    }
  }

  World.add(engine.world, blocks);
}

// 포탄을 1개 생성합니다.
function spawnProjectile() {
  currentProjectile = Bodies.circle(
    CONFIG.launcherX,
    CONFIG.launcherY,
    CONFIG.projectileRadius,
    {
      restitution: 0.5,
      friction: 0.01,
      density: 0.004,
      label: 'projectile',
    }
  );

  World.add(engine.world, currentProjectile);
}

// 마우스로 포탄을 끌 수 있게 제약을 만듭니다.
function setupMouseControl() {
  const mouse = Mouse.create(canvas);

  mouseConstraint = MouseConstraint.create(engine, {
    mouse,
    constraint: {
      stiffness: 0.08,
      render: { visible: false },
    },
  });

  World.add(engine.world, mouseConstraint);

  // 클릭 시작: 현재 포탄만 잡을 수 있게 체크
  Events.on(mouseConstraint, 'startdrag', (event) => {
    if (event.body === currentProjectile) {
      isDraggingProjectile = true;
    }
  });

  // 마우스를 놓았을 때: 포탄 발사 처리
  Events.on(mouseConstraint, 'enddrag', (event) => {
    if (event.body !== currentProjectile || !isDraggingProjectile || stageCleared || gameOver) return;

    isDraggingProjectile = false;

    // 현재 포탄 위치와 발사대 위치의 차이로 발사 속도를 계산
    const dx = CONFIG.launcherX - currentProjectile.position.x;
    const dy = CONFIG.launcherY - currentProjectile.position.y;

    const dragDistance = Math.hypot(dx, dy);
    const clampedDistance = Math.min(dragDistance, CONFIG.maxDragDistance);

    // 방향 벡터(정규화)
    const nx = dragDistance === 0 ? 0 : dx / dragDistance;
    const ny = dragDistance === 0 ? 0 : dy / dragDistance;

    // 당긴 거리 비율에 따라 힘을 계산
    const power = clampedDistance * 0.0009;

    // 포탄에 힘 적용 => 발사
    Body.applyForce(currentProjectile, currentProjectile.position, {
      x: nx * power,
      y: ny * power,
    });

    // 탄약 1개 사용
    ammo -= 1;
    updateUI();
  });
}

// 블록이 화면 아래로 떨어졌으면 제거 + 점수 증가
function cleanupFallenBlocks() {
  let removedCount = 0;

  blocks = blocks.filter((block) => {
    const isOut = block.position.y > canvas.height + 80;

    if (isOut) {
      World.remove(engine.world, block);
      removedCount += 1;
      return false;
    }

    return true;
  });

  if (removedCount > 0) {
    score += removedCount * 100;
    updateUI();
  }
}

// 포탄이 멈췄거나 화면 밖이면 다음 포탄 준비
function handleProjectileLifecycle() {
  if (!currentProjectile || stageCleared || gameOver) return;

  const speed = currentProjectile.speed;
  const outOfMap =
    currentProjectile.position.x < -100 ||
    currentProjectile.position.x > canvas.width + 100 ||
    currentProjectile.position.y > canvas.height + 120;

  const verySlow = speed < 0.15;

  // 발사한 이후 충분히 멈췄거나 밖으로 나간 경우 교체
  if ((ammo >= 0 && (outOfMap || verySlow)) && !isDraggingProjectile) {
    World.remove(engine.world, currentProjectile);
    currentProjectile = null;

    // 탄이 남아 있으면 새 포탄 생성
    if (ammo > 0) {
      spawnProjectile();
    }
  }
}

// 승리/패배 상태 확인
function checkGameState() {
  if (!stageCleared && blocks.length === 0) {
    stageCleared = true;
    messageEl.textContent = '🎉 스테이지 클리어! 모든 블록을 무너뜨렸습니다!';
    messageEl.className = 'message clear';
    return;
  }

  // 탄약이 0이고, 현재 포탄도 없고, 블록이 남아 있으면 게임오버
  if (!stageCleared && ammo <= 0 && !currentProjectile && blocks.length > 0) {
    gameOver = true;
    messageEl.textContent = '💥 게임오버! 탄이 모두 소진되었습니다. 재시작해 주세요.';
    messageEl.className = 'message over';
  }
}

// UI 텍스트 업데이트
function updateUI() {
  ammoEl.textContent = String(Math.max(ammo, 0));
  scoreEl.textContent = String(score);
  blocksLeftEl.textContent = String(blocks.length);
}

// 사각형/원 그리기 함수
function drawBody(body) {
  ctx.beginPath();

  // 원(포탄)인지 사각형(블록/벽)인지 확인
  if (body.circleRadius) {
    ctx.arc(body.position.x, body.position.y, body.circleRadius, 0, Math.PI * 2);
    ctx.fillStyle = '#333a49';
    ctx.fill();
    ctx.lineWidth = 2;
    ctx.strokeStyle = '#111722';
    ctx.stroke();
    return;
  }

  // 다각형(사각형 포함) 그리기
  const vertices = body.vertices;
  ctx.moveTo(vertices[0].x, vertices[0].y);
  for (let i = 1; i < vertices.length; i += 1) {
    ctx.lineTo(vertices[i].x, vertices[i].y);
  }
  ctx.closePath();

  if (body.label === 'building-block') {
    ctx.fillStyle = '#c98752';
  } else if (body.label === 'floor') {
    ctx.fillStyle = '#6f8f61';
  } else {
    ctx.fillStyle = '#5a6f9a';
  }

  ctx.fill();
  ctx.lineWidth = 1.5;
  ctx.strokeStyle = '#263449';
  ctx.stroke();
}

// 조준선(발사 방향 안내)을 그립니다.
function drawAimGuide() {
  if (!currentProjectile || !isDraggingProjectile) return;

  ctx.beginPath();
  ctx.moveTo(CONFIG.launcherX, CONFIG.launcherY);
  ctx.lineTo(currentProjectile.position.x, currentProjectile.position.y);
  ctx.strokeStyle = '#ff4f6d';
  ctx.lineWidth = 3;
  ctx.stroke();
}

// 매 프레임 화면 렌더링
function render() {
  ctx.clearRect(0, 0, canvas.width, canvas.height);

  // 발사대 표시
  ctx.beginPath();
  ctx.arc(CONFIG.launcherX, CONFIG.launcherY, 8, 0, Math.PI * 2);
  ctx.fillStyle = '#1f2d46';
  ctx.fill();

  // 월드 내 모든 물체를 그립니다.
  engine.world.bodies.forEach((body) => drawBody(body));

  // 조준선 그리기
  drawAimGuide();
}

// 게임 루프
function gameLoop() {
  Engine.update(engine, 1000 / 60); // 60FPS 물리 업데이트

  cleanupFallenBlocks();
  handleProjectileLifecycle();
  checkGameState();
  render();

  requestAnimationFrame(gameLoop);
}

// 월드를 초기화하고 새 게임을 시작
function startGame() {
  // 기존 물체 전체 제거
  World.clear(engine.world, false);

  // 상태 리셋
  ammo = CONFIG.maxAmmo;
  score = 0;
  stageCleared = false;
  gameOver = false;
  isDraggingProjectile = false;
  messageEl.textContent = '건물을 모두 무너뜨리세요!';
  messageEl.className = 'message';

  createBoundaries();
  createBuilding();
  spawnProjectile();

  updateUI();

  // 마우스 제약은 최초 1번만 생성
  if (!mouseConstraint) {
    setupMouseControl();
  }
}

// 재시작 버튼 연결
restartBtn.addEventListener('click', startGame);

// 최소 동작 버전: 페이지 로드시 바로 실행
startGame();
gameLoop();
