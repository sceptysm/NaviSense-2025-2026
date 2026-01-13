# Way of Working
## A Guide to Our Collaborative Process

This document outlines the development methodology, communication channels, and collaborative practices for our project. The aim is to agree on shared methods of working to improve collaboration efficiency.

The key topics covered are:
- Two-Week Sprint Cycle
- Communication Channels
- Knowledge Management

---

## Two-Week Sprint Cycle

The project follows an agile development framework centered around **two-week sprints**. The team works backwards from high-level objectives, breaking them into manageable tasks while allowing flexibility to adapt to issues as they arise.

### Sprint Phases

#### 1. Sprint Planning
At the beginning (or end) of each two-week cycle, the team reviews the project backlog and selects tasks for the upcoming sprint. The goal is to ensure:
- A realistic workload
- Clear and well-defined objectives

#### 2. Development
During the sprint, development proceeds according to the communication and coding guidelines defined in later sections.

#### 3. Sprint Review
At the end of each sprint, the team conducts the following activities:

**a. Review**  
Each developer presents a brief demo of their work and records any feedback from the client or team members.

**b. Retrospective**  
The team reflects on:
- What worked well
- What could be improved
- Any obstacles encountered

**c. Backlog Update**  
Completed items are signed off, and new tasks are added based on sprint outcomes.

### Project Backlog

 The sprint process relies on a **Project Backlog**, currently maintained on the Clickup platform.

Each backlog item should include:
- A brief description
- Priority level (High / Medium / Low)
- Estimated effort

Each backlog item must, most critically, be written in a testable manner - i.e. it should be easily understandable
whether a task is complete OR not. Simple binary

If a backlog item contains within it too many aspects to keep it reasonably simple, create additional subtask that decouple the responsibilities.
The benefit of the subtasks is exactly that they become more easily testable, and division of tasks more paletable.


---

## Communication Channels

The team uses multiple communication channels to keep discussions organized and easy to reference.

---

### Discord: Development Hub

Discord serves as the primary development communication platform.

#### 1. Formal Discussions Log

All technical and design discussions should be logged in a dedicated **Discussions** channel to create a searchable decision history. This also applies to development issues where feedback from the team is needed.

Each discussion should follow this structure:

- **Discussion Topic**  
  Brief description of the topic  
  _Example: Issue A – User Authentication Problem_

- **Topic Description**  
  Clear scope of the issue  
  _Example: As part of feature X, we must implement user authentication. I am encountering problem Y._

- **Discussion Messages**  
  Messages exchanged between team members

- **Decision Summary**  
  A final post summarizing the agreed outcome of the discussion

#### 2. Information Hub

The main Discord channel contains links to essential project resources, including:
- GitHub repository
- OneDrive
- Other relevant tools


### WhatsApp: Social and Personal Coordination

WhatsApp is reserved for informal and logistical communication, including:
- Checking in on team well-being
- Coordinating meetings
- Non-technical discussions



### Communication with the Client

- Client communication occurs at the **start of each sprint cycle**
- During the **first sprint**, objectives are communicated via email to:  
  j.pintado@student.tue.nl
- For subsequent sprints, emails should include:
  - Progress update from the previous sprint
  - Objectives for the upcoming sprint

**Email subject format:**

Sprint Cycle # Update

(where # is the sprint number)

#### Monthly Updates
- Updates on other events (e.g., TU/e contest) are sent on the **first day of each month**
- Updates should include current progress and reflections

#### Meetings
Full-team meetings occur:
- At the beginning of the year
- Before the Midterm Presentation
- Before Demo Day

Additional questions or requests where the client can help may be handled via WhatsApp.

---

## Knowledge Management

This section describes how the team captures and maintains knowledge throughout the project.


### 1. DocTree

The DocTree is the definitive version of all the documentation kept for this project. Guides, decisions, design, project descriptions - these types of documents can all be found inside the DocTree.

_TODO: Guide to adding new documents, guide on writing docs.  


### 2. Cloud OneDrive

The shared OneDrive stores all **non-code project assets**.

**Contents include:**
- **Project Foundations** – project descriptions, user personas, and long-term vision documents
- **Past Context** – reports and materials from previous years
- **Research and Research Briefs** – sources and research materials
- **Design and Architecture** – detailed architecture and design documents
- **Organizational Documents** – project backlog and sprint planning files



### 3. GitHub Repository

All code is managed through a shared GitHub repository using a structured workflow.

#### a. Feature Branch Workflow
New features must be developed on a branch created from the main development branch
Branches should follow the naming convention:
  feature-x-branch  
  Example: user-login-branch

#### b. Merge Requests
Once a feature is complete and tested locally, a merge request is opened.
Each merge request must include:
  - A clear description of the changes
  - Relevant context for reviewers

#### c. Meaningful Commit Messages

Clear and descriptive commit messages are mandatory.

For significant feature work, commit messages should follow this structure:

[Task or Feature Name]

Feature Description:
I implemented the user registration endpoint by creating a new service and repository in the auth module.

Next Steps:
To complete the user login feature, I still need to implement token generation.

#### d. Archive Old Branch
Once a merge request with a feature is passed, archive the old branch to keep the repository clean. 
