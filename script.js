const {
  Engine,
  Render,
  Runner,
  Bodies,
  Composite,
  Composites,
  Mouse,
  MouseConstraint,
} = Matter;

const width = 800;
const height = 520;

const engine = Engine.create();
const world = engine.world;

const render = Render.create({
  element: document.getElementById("world"),
  engine,
  options: {
    width,
    height,
    wireframes: false,
    background: "#0b1220",
  },
});

const ground = Bodies.rectangle(width / 2, height - 10, width, 20, {
  isStatic: true,
  render: { fillStyle: "#475569" },
});

const leftWall = Bodies.rectangle(0, height / 2, 20, height, { isStatic: true });
const rightWall = Bodies.rectangle(width, height / 2, 20, height, { isStatic: true });

const building = Composites.stack(280, 260, 6, 8, 0, 0, (x, y) =>
  Bodies.rectangle(x, y, 40, 40, {
    restitution: 0.05,
    friction: 0.8,
    render: { fillStyle: "#60a5fa" },
  })
);

Composite.add(world, [ground, leftWall, rightWall, building]);

function dropBall(x = 100) {
  const ball = Bodies.circle(x, 60, 22, {
    restitution: 0.7,
    friction: 0.01,
    render: { fillStyle: "#f59e0b" },
  });
  Composite.add(world, ball);
}

dropBall();

Render.run(render);
Runner.run(Runner.create(), engine);

const mouse = Mouse.create(render.canvas);
Composite.add(
  world,
  MouseConstraint.create(engine, {
    mouse,
    constraint: { stiffness: 0.2, render: { visible: false } },
  })
);
render.mouse = mouse;

render.canvas.addEventListener("click", (event) => {
  const rect = render.canvas.getBoundingClientRect();
  const x = event.clientX - rect.left;
  dropBall(x);
});
